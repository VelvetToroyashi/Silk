using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MediatR;
using Microsoft.Extensions.Logging;
using Silk.Core.Data.Entities;
using Silk.Core.Data.MediatR.Guilds;
using Silk.Core.Data.MediatR.Users;
using Silk.Extensions;
using Silk.Shared.Constants;

namespace Silk.Core.EventHandlers.Guilds
{
	public sealed class GuildCacher
	{

		private static readonly string _onGuildJoinThankYouMessage = "Hiya! My name is Silk! I hope to satisfy your entertainment and moderation needs." +
		                                                  $"\n\nI respond to mentions and `{StringConstants.DefaultCommandPrefix}` by default, but you can change that with `{StringConstants.DefaultCommandPrefix}prefix`" +
		                                                  "\n\nThere's also a variety of :sparkles: slash commands :sparkles: if those suit your fancy!" +
		                                                  "\n\nAlso! Development, hosting, infrastructure, etc. is expensive! " +
		                                                  "\nDonations via Ko-Fi *greatly* aid in this endeavour. <3";
		private readonly DiscordClient _client;
		private readonly ILogger<GuildCacher> _logger;

		private readonly IMediator _mediator;
		private readonly HashSet<ulong> _guilds = new();
		private long _guildCount;
		
		private DateTime? _startTime;
		public GuildCacher(IMediator mediator, DiscordClient client, ILogger<GuildCacher> logger)
		{
			_mediator = mediator;
			_client = client;
			_logger = logger;
		}

		internal async Task CacheGuildAsync(DiscordGuild guild)
		{
			_startTime ??= DateTime.Now;
			await _mediator.Send(new GetOrCreateGuildRequest(guild.Id, StringConstants.DefaultCommandPrefix));

			int members = await CacheMembersAsync(guild.Members.Values);

			if (_guilds.Add(guild.Id))
			{
				var current = Interlocked.Increment(ref _guildCount);
				
				LogMembers(members, guild.Members.Count, current);
			}
		}

		internal async Task JoinedGuild(GuildCreateEventArgs args)
		{
			_logger.LogInformation(EventIds.Service, "Joined new guild! {GuildName} | {GuildMemberCount} members", args.Guild.Name, args.Guild.MemberCount);

			DiscordMember? bot = args.Guild.CurrentMember;

			DiscordChannel? thankYouChannel = args.Guild
				.Channels.Values
				.OrderBy(c => c.Position)
				.FirstOrDefault(c => c.Type is ChannelType.Text && c.PermissionsFor(bot).HasPermission(Permissions.SendMessages | Permissions.EmbedLinks));

			if (thankYouChannel is not null)
			{
				DiscordEmbedBuilder? embed = new DiscordEmbedBuilder()
					.WithTitle("Thank you for adding me!")
					.WithColor(new("94f8ff"))
					.WithDescription(_onGuildJoinThankYouMessage)
					.WithThumbnail("https://files.velvetthepanda.dev/silk.png")
					.WithFooter("Silk! | Made by Velvet & Contributors w/ <3");

				DiscordMessageBuilder? builder = new DiscordMessageBuilder()
					.WithEmbed(embed)
					.AddComponents(new DiscordLinkButtonComponent("https://ko-fi.com/velvetthepanda", "Ko-Fi!"),
						new DiscordLinkButtonComponent("https://discord.gg/HZfZb95", "Support server!"),
						new DiscordLinkButtonComponent($"https://discord.com/api/oauth2/authorize?client_id={_client.CurrentApplication.Id}&permissions=502656214&scope=bot%20applications.commands", "Invite me!"),
						new DiscordLinkButtonComponent("https://github.com/VelvetThePanda/Silk", "Source code!"));

				await thankYouChannel.SendMessageAsync(builder);
			}
			
			await CacheGuildAsync(args.Guild);
		}
		
		private void LogMembers(int members, int totalMembers, long currentGuilds)
		{
			string message;
			message = members is 0 ?
				"Guild cached! Shard [{Shard}/{Shards}] → Guild [{CurrentGuild}/{Guilds}]" :
				"Guild cached! Shard [{Shard}/{Shards}] → Guild [{CurrentGuild}/{Guilds}] → Staff [{Members}/{AllMembers}]";
			
			_logger.LogDebug(EventIds.EventHandler, message, _client.ShardId + 1, _client.ShardCount, currentGuilds, _client.Guilds.Count, members, totalMembers);
		}

		private async Task<int> CacheMembersAsync(IEnumerable<DiscordMember> members)
		{
			var staffCount = 0;
			List<DiscordMember> staff = members.Where(m => !m.IsBot && m.Permissions.HasPermission(FlagConstants.CacheFlag)).ToList();

			foreach (var member in staff)
			{
				UserFlag flag = member.HasPermission(Permissions.Administrator) || member.IsOwner ? UserFlag.EscalatedStaff : UserFlag.Staff;

				UserEntity? user = await _mediator.Send(new GetUserRequest(member.Guild.Id, member.Id));
				if (user is not null)
				{
					if (!user.Flags.Has(flag))
					{
						staffCount++;
						user.Flags.Add(flag);
					}
					await _mediator.Send(new UpdateUserRequest(member.Guild.Id, member.Id, user.Flags));
				}
				else
				{
					await _mediator.Send(new AddUserRequest(member.Guild.Id, member.Id, flag));
					staffCount++;
				}
			}
			return Math.Max(staffCount, 0);
		}
	}
}
