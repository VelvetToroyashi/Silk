using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Silk.Interactivity;

public class InteractivityResponder<T> : IResponder<T> where T : IGatewayEvent
{
    private readonly InteractivityWaiter<T> _waiter;
    public InteractivityResponder(InteractivityWaiter<T> waiter)
        => _waiter = waiter;

    public  Task<Result> RespondAsync(T gatewayEvent, CancellationToken ct = default)
    {
        _waiter.TryEvaluateEvents(gatewayEvent);
        return Task.FromResult(Result.FromSuccess());
    }
}