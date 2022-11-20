using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Silk.Infrastructure;

public class ScopedHostedServiceLoaderService : IHostedService
{
    private readonly IServiceProvider _services;
    public ScopedHostedServiceLoaderService(IServiceProvider services)
    {
        _services = services;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _services.CreateAsyncScope();

        var scopedServices = scope.ServiceProvider.GetServices<IScopedHostedService>();
        
        foreach (var scopedService in scopedServices)
        {
            await scopedService.StartAsync(cancellationToken);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await using var scope = _services.CreateAsyncScope();
        
        var scopedServices = scope.ServiceProvider.GetServices<IScopedHostedService>();
        
        foreach (var scopedService in scopedServices)
        {
            await scopedService.StopAsync(cancellationToken);
        }
    }
}