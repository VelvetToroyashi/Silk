using System.Threading.Tasks;
using AnnoucementPlugin.Utilities;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Logging;

namespace AnnoucementPlugin.Services
{
	public sealed class MessageDispatcher : IMessageDispatcher
	{
		private readonly DiscordClient _client;
		private readonly ILogger<MessageDispatcher> _logger;
		
		public MessageDispatcher(ILogger<MessageDispatcher> logger, DiscordClient client)
		{
			_logger = logger;
			_client = client;
		}
		
		public async Task<MessageSendResult> DispatchMessage(ulong guild, ulong channel, string message)
		{
			var exists = AnnouncementChannelExists(guild, channel);

			if (!exists)
				return new(false, MessageSendErrorType.ChannelDoesNotExist);
			
			var unlocked = await UnlockChannelAsync(guild, channel);

			if (!unlocked)
				return new(false, MessageSendErrorType.CouldNotUnlockChannel);

			var messageChannel = _client.Guilds[guild].Channels[channel];
			
			if (message.Length <= 2000)
			{
				try
				{
					await messageChannel.SendMessageAsync(message);
				}
				catch
				{
					return new(false, MessageSendErrorType.Unknown);
				}
			}
			else
			{
				var embed = new DiscordEmbedBuilder()
					.WithColor(DiscordColor.Azure)
					.WithDescription(message);
				
				try
				{
					await messageChannel.SendMessageAsync(embed);
				}
				catch
				{
					return new(false, MessageSendErrorType.Unknown);
				}
			}

			return new(true);
		}
		
		
		private async Task<bool> UnlockChannelAsync(ulong guildId, ulong channelId)
		{
			var guild = _client.Guilds[guildId];
			var channel = guild.GetChannel(channelId);


			var canSendMessages = channel.PermissionsFor(guild.CurrentMember).HasPermission(Permissions.SendMessages);
			
			if (canSendMessages)
				return true;

			var roleCanModifyChannel = channel.PermissionsFor(guild.CurrentMember).HasPermission(Permissions.ManageChannels);
			var memberCanModifyChannel = channel.PermissionsFor(guild.CurrentMember).HasPermission(Permissions.ManageChannels);
			
			if (!roleCanModifyChannel && !memberCanModifyChannel)
				return false;

			try
			{
				await channel.AddOverwriteAsync(guild.CurrentMember, Permissions.SendMessages);
				return true;
			}
			catch (UnauthorizedException)
			{
				return false;
			}
		}
		
		private bool AnnouncementChannelExists(ulong guild, ulong channel)
		{
			var guildObj = _client.Guilds[guild];

			return guildObj.Channels.TryGetValue(channel, out _);
		}
	}
}