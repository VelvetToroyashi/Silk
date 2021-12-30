using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Gateway.Extensions;

namespace Silk.Interactivity;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddInteractivity(this IServiceCollection services)
    {
        services.AddSingleton<InteractivityExtension>();
        
        services.AddSingleton<InteractivityWaiter<IMessageCreate>>();
        services.AddSingleton<InteractivityWaiter<IInteractionCreate>>();


        services.AddResponder<InteractivityResponder<IMessageCreate>>();
        services.AddResponder<InteractivityResponder<IInteractionCreate>>();

        return services;
    }
}