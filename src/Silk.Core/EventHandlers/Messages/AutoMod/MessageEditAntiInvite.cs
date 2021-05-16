using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Silk.Core.Data.Models;
using Silk.Core.Services;
using Silk.Core.Services.Interfaces;

namespace Silk.Core.EventHandlers.Messages.AutoMod
{
    public class MessageEditAntiInvite
    {
        private readonly ConfigService _configService;
        private readonly IInfractionService _infractionService;

        public MessageEditAntiInvite(IInfractionService infractionService, ConfigService configService)
        {
            _infractionService = infractionService;
            _configService = configService;
        }

        public async Task CheckForInvite(DiscordClient client, MessageUpdateEventArgs args)
        {
            if (args.Channel.IsPrivate) return;
            GuildConfig config = await _configService.GetConfigAsync(args.Guild.Id);

            bool hasInvite = AntiInviteCore.CheckForInvite(client, args.Message, config, out string invite);
            bool isBlacklisted = await AntiInviteCore.IsBlacklistedInvite(client, args.Message, config, invite);

            if (hasInvite && isBlacklisted)
                await AntiInviteCore.TryAddInviteInfractionAsync(config, args.Message, _infractionService);
        }
    }
}