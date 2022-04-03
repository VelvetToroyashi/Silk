using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Gateway.Responders;
using Remora.Results;
using Silk.Utilities;

namespace Silk.Responders;

public class GatewayEventMetricResponder : IResponder<IGatewayEvent>
{
    public Task<Result> RespondAsync(IGatewayEvent gatewayEvent, CancellationToken ct = default)
    {
        var name = gatewayEvent.GetType().Name.Humanize(LetterCasing.Title);
        
        SilkMetric.GatewayEventReceieved.WithLabels(name).Inc();

        return Task.FromResult(Result.FromSuccess());
    }
}