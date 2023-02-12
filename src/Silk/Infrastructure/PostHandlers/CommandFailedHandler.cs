using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Services;
using Remora.Results;

namespace Silk;

public class CommandFailedHandler : IPreparationErrorEvent
{
    public Task<Result> PreparationFailed(IOperationContext context, IResult preparationResult, CancellationToken ct) => null;
}
