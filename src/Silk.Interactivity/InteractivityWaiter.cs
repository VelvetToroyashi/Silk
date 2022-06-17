using System.Reflection;
using System.Runtime.CompilerServices;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Results;

namespace Silk.Interactivity;

public class InteractivityWaiter
{
    private readonly List<InteractivityRequest> _events = new();

    internal void TryEvaluateEvents(IGatewayEvent gatewayEvent)
    {
        if (!_events.Any())
            return;
        
        for (var i = _events.Count - 1; i >= 0; i--)
        {
            var request = _events[i];
            if (!gatewayEvent.GetType().GetInterfaces().Contains(request.GetType().GenericTypeArguments[0]))
                continue;

            var wait      = request.GetType().GetProperty("Wait", BindingFlags.Public      | BindingFlags.Instance)!.GetValue(request)!;
            var predicate = request.GetType().GetProperty("Predicate", BindingFlags.Public | BindingFlags.Instance)!.GetValue(request)!;

            if (Unsafe.As<Func<IGatewayEvent, bool>>(predicate)(gatewayEvent))
            {
                Unsafe.As<TaskCompletionSource<IGatewayEvent>>(wait).TrySetResult(gatewayEvent);
                _events.Remove(request);
            }
        }
    }
    
    public async Task<Result<T?>> WaitForEventAsync<T>(Func<T, bool> predicate, CancellationToken ct = default) where T : IGatewayEvent
    {
        var tcs = new TaskCompletionSource<Result<T?>>(ct);
        _events.Add(new InteractivityRequest<T>(tcs, predicate));

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