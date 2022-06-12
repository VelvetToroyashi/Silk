using Microsoft.Extensions.DependencyInjection;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Gateway.Extensions;

namespace Silk.Interactivity;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddSilkInteractivity(this IServiceCollection services)
    {
        services.AddSingleton<InteractivityExtension>();

        services.AddSingleton<InteractivityWaiter>();
        services.AddResponder<InteractivityResponder>();
        
        return services;
    }
}