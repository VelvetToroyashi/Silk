using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Silk.Core.Data.MediatR.Guilds;
using Silk.Core.Data.Models;
using Silk.Core.Discord.EventHandlers.Notifications;
using Silk.Core.Discord.Services.Interfaces;

namespace Silk.Core.Discord.EventHandlers.MessageAdded.AutoMod
{
    public class MessageAddAntiInvite : INotificationHandler<MessageCreated>
    {

        private readonly IInfractionService _infractionService;
        private readonly IMediator _mediator;

        public MessageAddAntiInvite(IInfractionService infractionService, IMediator mediator)
        {
            _infractionService = infractionService;
            _mediator = mediator;
        }

        public async Task Handle(MessageCreated notification, CancellationToken cancellationToken)
        {
            if (notification.Event.Channel.IsPrivate) return;
            GuildConfig config = await _mediator.Send(new GetGuildConfigRequest(notification.Event.Guild!.Id), cancellationToken);
            bool hasInvite = await AntiInviteCore.CheckForInviteAsync(notification.Client, notification.Event.Message, config);

            if (hasInvite)
                await AntiInviteCore.TryAddInviteInfractionAsync(config, notification.Event.Message, _infractionService);
        }
    }
}