using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Services;
using Remora.Results;
using Silk.Utilities;

namespace Silk;

public class CommandMetricsHandler : IPostExecutionEvent
{
    public Task<Result> AfterExecutionAsync(ICommandContext context, IResult commandResult, CancellationToken ct)
    {
        var commandType = context is MessageContext ? "message" : "slash";

        SilkMetric.SeenCommands.WithLabels(commandType).Inc();

        return Task.FromResult(Result.FromSuccess());
    }
}