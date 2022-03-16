using AspNet.Security.OAuth.Discord;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Remora.Discord.Rest.Extensions;
using Silk.Dashboard.Extensions;
using Silk.Dashboard.Services.DashboardDiscordClient;
using Silk.Dashboard.Services.DashboardDiscordClient.Interfaces;
using Silk.Dashboard.Services.DiscordTokenStorage;
using Silk.Dashboard.Services.DiscordTokenStorage.Interfaces;
using Silk.Data;
using Silk.Shared.Configuration;
using Silk.Shared.Constants;

namespace Silk.Dashboard;

public static class DependencyInjection
{
    public static IServiceCollection AddDashboardInfrastructure
    (
        this IServiceCollection services,
        IConfiguration          configuration
    )
    {
        var silkConfigOptions = configuration.GetSilkConfigurationOptions();
        return services
              .AddSilkConfigurationOptions(configuration)
              .AddSilkDatabase(silkConfigOptions)
              .AddDashboardServices()
              .AddDashboardDiscordRest()
              .AddDashboardDiscordAuthentication(silkConfigOptions);
    }

    private static IServiceCollection AddDashboardDiscordRest
    (
        this IServiceCollection services
    )
    {
        return services.AddDiscordRest(_ => "_", clientBuilder => 
        { 
            clientBuilder.ConfigureHttpClient((provider, client) => 
            {
                // Todo: Check that using the context accessor works (NOT recommended)
                // due to HttpContext only hitting _Host.cshtml page on initial request or full page reload)
                var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
                var tokenStore          = provider.GetRequiredService<IDiscordTokenStore>();

                var userId      = httpContextAccessor.HttpContext?.User.GetUserId();
                var accessToken = tokenStore.GetToken(userId!)?.AccessToken;

                client.DefaultRequestHeaders.Authorization = new("Bearer", accessToken); 
            });
        });
    }

    private static IServiceCollection AddDashboardDiscordAuthentication
    (
        this IServiceCollection  services,
        SilkConfigurationOptions silkConfigOptions
    )
    {
        services.AddAuthentication(opt => 
                 {
                     opt.DefaultScheme          = CookieAuthenticationDefaults.AuthenticationScheme;
                     opt.DefaultSignInScheme    = CookieAuthenticationDefaults.AuthenticationScheme; 
                     opt.DefaultChallengeScheme = DiscordAuthenticationDefaults.AuthenticationScheme;
                 })
                .AddDiscord(opt => 
                 {
                     opt.UsePkce = true; 
                     opt.SaveTokens = true; 
                     
                     opt.Scope.Add("guilds");
                     
                     opt.ClientId          = silkConfigOptions.Discord.ClientId; 
                     opt.ClientSecret      = silkConfigOptions.Discord.ClientSecret;
                     
                     opt.CallbackPath       = DiscordAuthenticationDefaults.CallbackPath; 
                     opt.AccessDeniedPath   = new("/");
                     opt.ReturnUrlParameter = string.Empty;
                     
                     opt.Events.OnCreatingTicket = context => 
                     {
                         var tokenStore = context.HttpContext.RequestServices.GetRequiredService<IDiscordTokenStore>(); 
                         
                         var userId      = context.Principal.GetUserId();
                         
                         var tokenExpiry = DiscordTokenStoreExtensions.GetTokenExpiry(context);
                         
                         var tokenEntry = new DiscordTokenStoreEntry(context.AccessToken,
                                                                     context.RefreshToken,
                                                                     tokenExpiry,
                                                                     context.TokenType); 
                         tokenStore.SetToken(userId!, tokenEntry); 
                         return Task.CompletedTask;
                     };
                 })
                .AddCookie(options => 
                 { 
                     // Todo: Find way to set expiration based on OAuth token expiry
                     options.ExpireTimeSpan = TimeSpan.FromDays(7); 
                 });

        return services;
    }

    private static IServiceCollection AddSilkConfigurationOptions
    (
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        return services.Configure<SilkConfigurationOptions>(configuration.GetSection(SilkConfigurationOptions.SectionKey));
    }

    private static IServiceCollection AddSilkDatabase
    (
        this IServiceCollection  services,
        SilkConfigurationOptions silkConfigOptions
    )
    {
        return services.AddDbContextFactory<GuildContext>(dbBuilder =>
        {
            dbBuilder.UseNpgsql(silkConfigOptions.Persistence.GetConnectionString());
#if DEBUG
            dbBuilder.EnableSensitiveDataLogging();
            dbBuilder.EnableDetailedErrors();
#endif
        });
    }

    private static IServiceCollection AddDashboardServices
    (
        this IServiceCollection services
    )
    {
        services.AddHttpClient("Github", con => 
        {
            con.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/vnd.github.v3+json");
            con.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", StringConstants.ProjectIdentifier);
        });

        services.AddMudServices();
        
        services.AddMediatR(typeof(GuildContext));

        services.AddScoped<IDashboardDiscordClient, DashboardDiscordClient>();
        
        services.AddSingleton<IDiscordTokenStore, DiscordTokenStore>();
        services.AddSingleton<IDiscordTokenStoreWatcher, DiscordTokenStoreWatcher>();
        services.AddHostedService(s => s.GetRequiredService<IDiscordTokenStoreWatcher>());
        
        return services;
    }

    private static SilkConfigurationOptions GetSilkConfigurationOptions
    (
        this IConfiguration configuration
    )
    {
        // Todo: Consolidate Adding SilkConfigurationOptions to common location?
        return configuration.GetSection(SilkConfigurationOptions.SectionKey).Get<SilkConfigurationOptions>();
    }
}