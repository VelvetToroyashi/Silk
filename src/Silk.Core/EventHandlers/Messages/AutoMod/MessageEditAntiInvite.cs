using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Silk.Core.Data.Entities;
using Silk.Core.Services.Data;

namespace Silk.Core.EventHandlers.Messages.AutoMod
{
    public class MessageEditAntiInvite
    {
        private readonly ConfigService    _configService;
        private readonly AntiInviteHelper _inviteHelper;
        public MessageEditAntiInvite(ConfigService configService, AntiInviteHelper inviteHelper)
        {
            _configService = configService;
            _inviteHelper = inviteHelper;
        }

        public async Task CheckForInvite(DiscordClient client, MessageUpdateEventArgs args)
        {
            if (args.Channel.IsPrivate) return;
            GuildModConfigEntity? config = await _configService.GetModConfigAsync(args.Guild.Id);
            bool hasInvite = _inviteHelper.CheckForInvite(args.Message, config, out string invite);
            bool isBlacklisted = await _inviteHelper.IsBlacklistedInvite(args.Message, config, invite);

            if (hasInvite && isBlacklisted)
                await _inviteHelper.TryAddInviteInfractionAsync(args.Message, config);
        }
    }
}