using MediatR;

namespace Silk.Core.Discord.EventHandlers.Notifications
{
    public record MemberJoined : INotification { }

    public record MemberRemoved : INotification { }

    public record MemberUpdated : INotification { }

    public record MemberStatusUpdated : INotification { }

}