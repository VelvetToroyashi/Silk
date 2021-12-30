using System.Collections.Concurrent;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Results;

namespace Silk.Interactivity;

public class InteractivityWaiter<T> where T : IGatewayEvent
{
    private readonly ConcurrentBag<InteractivityRequest<T>> _events = new();

    public void TryEvaluateEvents(T gatewayEvent)
    {
        foreach (var request in _events)
            if (request.Predicate(gatewayEvent))
                request.Wait.TrySetResult(gatewayEvent);
    }
    
    public async Task<Result<T?>> WaitForEventAsync(Func<T, bool> predicate, CancellationToken ct = default)
    {
        var tcs = new TaskCompletionSource<Result<T?>>(ct);
        _events.Add(new(tcs, predicate));

        try
        {
            return await tcs.Task;
        }
        catch (OperationCanceledException)
        {
            return Result<T?>.FromSuccess(default);
        }
        catch (Exception e)
        {
            return Result<T?>.FromError(e);
        }
    }
}