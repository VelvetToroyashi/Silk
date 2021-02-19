using System;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Silk.Core.Commands.Tests
{
    public class NotifyCommand : BaseCommandModule
    {
        private readonly IMediator _mediator;
        public NotifyCommand(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        [Command]
        public async Task Notify(CommandContext ctx)
        {
            Console.WriteLine(DateTime.Now);
            _ = Task.Run(async () => _mediator.Publish(new MessageNotification()));
            Console.WriteLine(DateTime.Now);
        }
    }

    public class MessageNotification : INotification { }

    public class MsgNotifHandler1 : INotificationHandler<MessageNotification>
    {
        private readonly ILogger<MsgNotifHandler1> _logger;
        public MsgNotifHandler1(ILogger<MsgNotifHandler1> logger)
        {
            _logger = logger;
        }
            
        public async Task Handle(MessageNotification notification, CancellationToken cancellationToken)
        {
            await Task.Delay(1000);
            _logger.LogTrace($"Caught notification! Time: {DateTime.Now.Ticks}");    
        }
    }
    
    public class MsgNotifHandler2 : INotificationHandler<MessageNotification>
    {
        private readonly ILogger<MsgNotifHandler2> _logger;
        public MsgNotifHandler2(ILogger<MsgNotifHandler2> logger)
        {
            _logger = logger;
        }
            
        public async Task Handle(MessageNotification notification, CancellationToken cancellationToken)
        {
            await Task.Delay(1000);
            _logger.LogTrace($"Caught notification! Time: {DateTime.Now.Ticks}");    
        }
    }
    
}