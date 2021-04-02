using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Silk.Core.Data.Models;
using Silk.Core.Discord.EventHandlers.Notifications;
using Silk.Core.Discord.Services;
using Silk.Core.Discord.Services.Interfaces;

namespace Silk.Core.Discord.EventHandlers.MessageAdded.AutoMod
{
    public class MessageEditAntiInvite : INotificationHandler<MessageEdited>
    {

        private readonly IInfractionService _infractionService;
        private readonly ConfigService _configService;
        private readonly IMediator _mediator;

        public MessageEditAntiInvite(IInfractionService infractionService, IMediator mediator, ConfigService configService)
        {
            _infractionService = infractionService;
            _mediator = mediator;
            _configService = configService;
        }

        public async Task Handle(MessageEdited notification, CancellationToken cancellationToken)
        {
            if (notification.EventArgs.Channel.IsPrivate) return;
            GuildConfig config = await _configService.GetConfigAsync(notification.EventArgs.Guild.Id);
            bool hasInvite = await AntiInviteCore.CheckForInviteAsync(notification.Client, notification.EventArgs.Message, config);

            if (hasInvite)
                await AntiInviteCore.TryAddInviteInfractionAsync(config, notification.EventArgs.Message, _infractionService);
        }
    }
}