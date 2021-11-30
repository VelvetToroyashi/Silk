using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Gateway;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Core.Data.Entities;
using Silk.Core.Data.MediatR.Guilds;
using Silk.Core.Data.MediatR.Users;
using Silk.Shared.Constants;

namespace Silk.Core.Services.Data
{
	public class GuildCacherService
	{
		private const string _onGuildJoinThankYouMessage = "Hiya! My name is Silk! I hope to satisfy your entertainment and moderation needs.\n\n" +
		                                                   $"I respond to mentions and `{StringConstants.DefaultCommandPrefix}` by default, " +
		                                                   $"but you can change that with `{StringConstants.DefaultCommandPrefix}prefix`\n\n" +
		                                                   "There's also a variety of :sparkles: slash commands :sparkles: if those suit your fancy!";
		private readonly IDiscordRestChannelAPI _channelApi;
		private readonly DiscordGatewayClient _gateway;


		/// <summary>
		///     A collection of guilds known to be already joined. Populated on READY gateway event.
		/// </summary>
		private readonly HashSet<Snowflake> _knownGuilds = new();

		private readonly IMediator _mediator;
		public GuildCacherService(IMediator mediator, DiscordGatewayClient gateway, IDiscordRestChannelAPI channelApi)
		{
			_mediator = mediator;
			_gateway = gateway;
			_channelApi = channelApi;
		}




		/// <summary>
		///     Stores hashes of guilds that are available during READY to differientiate from joining a new guild.
		/// </summary>
		/// <param name="ready">The gateway event to store IDs from.</param>
		/// <returns></returns>
		public Result StoreKnownGuilds(IReady ready)
		{
			var failed = false;
			var errors = new List<IResult>();

			foreach (var guild in ready.Guilds)
			{
				if (!_knownGuilds.Add(guild.GuildID))
				{
					failed = true;
					errors.Add(Result.FromError(new InvalidOperationError($"Guild {guild.GuildID} was already known.")));
				}
			}

			return failed ? Result.FromError(new AggregateError(errors, "One or more guilds were already known.")) : Result.FromSuccess();
		}

		/// <summary>
		///     Checks whether a guild was just joined, or if it was already joined.
		/// </summary>
		/// <param name="guildID">The ID of the guild to check.</param>
		/// <returns></returns>
		public bool IsNewGuild(Snowflake guildID)
		{
			return !_knownGuilds.Contains(guildID);
		}

		public async Task<Result> CacheGuildAsync(Snowflake guildID, IReadOnlyList<IGuildMember> members)
		{
			if (members.Count > 2) // Just us. Rip.
				return Result.FromError(new ArgumentOutOfRangeError("Members only contained current user."));

			GuildEntity? guild = await _mediator.Send(new GetOrCreateGuildRequest(guildID.Value, StringConstants.DefaultCommandPrefix));



			return default;
		}


		public async Task<Result> CacheMembersAsync(Snowflake guildID, IReadOnlyList<IGuildMember> members)
		{
			if (members.Count > 2) // Just us, or just a bad collection. Either way this isn't a valid state.
				return Result.FromError(new ArgumentOutOfRangeError("Members only contained current user."));

			var erroredMembers = new List<IResult>();

			foreach (var member in members)
			{
				if (!member.User.IsDefined(out IUser? user))
				{
					erroredMembers.Add(Result.FromError(new InvalidOperationError("Member did not have a defined user.")));
					continue;
				}

				if (user.IsBot.IsDefined(out bool bot) && bot)
					continue;

				bool eligible = member.Permissions.IsDefined(out IDiscordPermissionSet? permissions)
				                && permissions.HasPermission(DiscordPermission.KickMembers)
				                && permissions.HasPermission(DiscordPermission.ManageMessages);

				if (!eligible) continue;

				Result<UserEntity> result = await _mediator.Send(new GetOrCreateUserRequest(guildID.Value, user.ID.Value, UserFlag.Staff));


			}


			return erroredMembers.Any()
				? Result.FromError(new AggregateError(erroredMembers, "One or more guild members could not be cached."))
				: Result.FromSuccess();
		}
	}
}