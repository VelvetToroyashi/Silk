using System.Threading;
using System.Threading.Tasks;

namespace Silk.Infrastructure;

public interface IScopedHostedService
{
    /// <summary>
    /// Called when the host starts. This method should initialize the service.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token passed from the host. This will be cancelled in the event that the host shuts down.</param>
    /// <returns>The task.</returns>
    Task StartAsync(CancellationToken cancellationToken);
    
    /// <summary>
    /// Called when the host stops. This method should clean up the service.
    /// </summary>
    /// <param name="cancellationToken">The cancellaiton token passed from the host.</param>
    /// <returns>The task.</returns>
    Task StopAsync(CancellationToken cancellationToken);
}