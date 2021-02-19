using MediatR;

namespace Silk.Core.EventHandlers.Notifications
{
    public class MessageCreated : INotification { }
    public class MessageDeleted : INotification { }
    public class MessageUpdated : INotification { }
}