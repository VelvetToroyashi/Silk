using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Silk.Core.EventHandlers.Notifications;
using Silk.Core.Services.Interfaces;
using Silk.Data.MediatR;
using Silk.Data.Models;

namespace Silk.Core.EventHandlers.MessageAdded.AutoMod
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
            if (notification.EventArgs.Channel.IsPrivate) return;
            GuildConfig config = await _mediator.Send(new GuildConfigRequest.Get(notification.EventArgs.Guild.Id), cancellationToken);
            bool hasInvite = await AntiInviteCore.CheckForInviteAsync(notification.Client, notification.EventArgs.Message, config);
            
            if (hasInvite)
                await AntiInviteCore.TryAddInviteInfractionAsync(config, notification.EventArgs.Message, _infractionService);
        }
    }
}