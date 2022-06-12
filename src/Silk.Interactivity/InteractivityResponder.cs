using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Silk.Interactivity;

public class InteractivityResponder : IResponder<IGatewayEvent>
{
    private readonly InteractivityWaiter _waiter;
    public InteractivityResponder(InteractivityWaiter waiter)
        => _waiter = waiter;

    public Task<Result> RespondAsync(IGatewayEvent gatewayEvent, CancellationToken ct = default)
    {
        _waiter.TryEvaluateEvents(gatewayEvent);
        return Task.FromResult(Result.FromSuccess());
    }
}