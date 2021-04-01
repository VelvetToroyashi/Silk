using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Silk.Core.Discord.EventHandlers.Notifications;

namespace Silk.Core.Discord.EventHandlers
{
    public class MessageCountHandler : INotificationHandler<MessageCreated>
    {
        private int _messages = 0;

        public async Task Handle(MessageCreated notification, CancellationToken cancellationToken)
        {
            _messages++;
            if (_messages is 10)
            {
                if (!File.Exists("./MessagesSeen")) File.Create("./MessagesSeen");
                string currentMessages = await File.ReadAllTextAsync("./MessagesSeen", cancellationToken);
                int currentMessageCount = int.Parse(currentMessages) + _messages;
                await File.WriteAllTextAsync("./MessagesSeen", currentMessageCount.ToString(), cancellationToken);
                _messages = 0;
            }
        }
    }
}