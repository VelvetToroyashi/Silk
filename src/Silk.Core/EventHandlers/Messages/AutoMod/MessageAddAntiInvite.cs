using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using MediatR;
using Silk.Core.Data.Models;
using Silk.Core.Services.Data;
using Silk.Core.Services.Interfaces;

namespace Silk.Core.EventHandlers.Messages.AutoMod
{
    public class MessageAddAntiInvite
    {
        private readonly ConfigService _config;
        private readonly IModerationService _moderationService;
        private readonly IMediator _mediator;

        public MessageAddAntiInvite(IModerationService moderationService, IMediator mediator, ConfigService config)
        {
            _moderationService = moderationService;
            _mediator = mediator;
            _config = config;
        }

        public async Task CheckForInvite(DiscordClient client, MessageCreateEventArgs args)
        {
            if (!args.Channel.IsPrivate && args.Author != client.CurrentUser)
            {
                GuildConfig config = await _config.GetConfigAsync(args.Guild.Id);

                bool hasInvite = AntiInviteCore.CheckForInvite(client, args.Message, config, out string invite);
                bool isBlacklisted = await AntiInviteCore.IsBlacklistedInvite(client, args.Message, config, invite!);

                if (hasInvite && isBlacklisted)
                    await AntiInviteCore.TryAddInviteInfractionAsync(config, args.Message, _moderationService);
            }
        }
    }
}