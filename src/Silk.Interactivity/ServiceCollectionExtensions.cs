using Microsoft.Extensions.DependencyInjection;
using Remora.Discord.Gateway.Extensions;

namespace Silk.Interactivity;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSilkInteractivity(this IServiceCollection services)
    {
        services.AddSingleton<InteractivityExtension>();

        services.AddSingleton<InteractivityWaiter>();
        services.AddResponder<InteractivityResponder>();
        
        return services;
    }
}