using System.Threading.Tasks;
using Remora.Results;
using Silk.Data.Entities;

namespace Silk.Services.Interfaces;

public interface IInfractionServiceV2
{
    /// <summary>
    /// Queues an infraction to be handled.
    /// </summary>
    /// <param name="request">The request to queue.</param>
    /// <returns>A task that completes when the infraction is processed.</returns>
    public Task<Result<InfractionEntity>> SubmitInfractionAsync(InfractionRequest request);
}