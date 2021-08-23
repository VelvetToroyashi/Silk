using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Silk.Core.Data.Models;
using Silk.Core.Services.Data;

namespace Silk.Core.EventHandlers.Messages.AutoMod
{
	public sealed class MessageAddAntiInvite
	{
		private readonly ConfigService _config;
		private readonly AntiInviteHelper _inviteHelper;

		public MessageAddAntiInvite(ConfigService config, AntiInviteHelper inviteHelper)
		{
			_config = config;
			_inviteHelper = inviteHelper;
		}

		public async Task CheckForInvite(DiscordClient client, MessageCreateEventArgs args)
		{
			if (!args.Channel.IsPrivate && args.Author != client.CurrentUser)
			{
				GuildModConfig? config = await _config.GetModConfigAsync(args.Guild.Id);
				
				bool hasInvite = _inviteHelper.CheckForInvite(args.Message, config, out string invite);
				
				if (!hasInvite)
					return;
				
				bool isBlacklisted = await _inviteHelper.IsBlacklistedInvite(args.Message, config, invite);

				if (isBlacklisted)
					await _inviteHelper.TryAddInviteInfractionAsync(args.Message, config);
			}
		}
	}
}