using System.Security.Claims;
using AspNet.Security.OAuth.Discord;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MudBlazor;
using MudBlazor.Services;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Rest;
using Remora.Discord.Rest.Extensions;
using Remora.Rest.Core;
using Silk.Dashboard.Extensions;
using Silk.Dashboard.Models;
using Silk.Dashboard.Providers;
using Silk.Dashboard.Services;
using Silk.Data;
using Silk.Data.EntityMapping;
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
        services.AddDiscordRest
        (
         provider => (provider.GetRequiredService<IOptions<SilkConfigurationOptions>>()
                             .Value.Discord.BotToken, DiscordTokenType.Bot)
        );

        services.AddScoped<DashboardDiscordClient>();

        return services;
    }

    private static IServiceCollection AddDashboardDiscordAuthentication
    (
        this IServiceCollection  services,
        SilkConfigurationOptions silkConfigOptions
    )
    {
        services.AddAuthentication(opts => 
        {
            opts.DefaultScheme          = CookieAuthenticationDefaults.AuthenticationScheme;
            opts.DefaultSignInScheme    = CookieAuthenticationDefaults.AuthenticationScheme;
            opts.DefaultChallengeScheme = DiscordAuthenticationDefaults.AuthenticationScheme;
        })
        .AddDiscord(opts => 
        {
            opts.UsePkce    = true;
            opts.SaveTokens = true;

            opts.Scope.Add("guilds");

            opts.ClientId     = silkConfigOptions.Discord.ClientId;
            opts.ClientSecret = silkConfigOptions.Discord.ClientSecret;

            opts.CallbackPath       = DiscordAuthenticationDefaults.CallbackPath;
            opts.AccessDeniedPath   = new("/");
            opts.ReturnUrlParameter = string.Empty;

            opts.Events.OnCreatingTicket = async context =>
            {
                var serviceProvider = context.HttpContext.RequestServices;
                var tokenStore      = serviceProvider.GetRequiredService<DiscordTokenStore>();
                var oAuth2Api       = serviceProvider.GetRequiredService<IDiscordRestOAuth2API>();

                var userId          = context.Principal!.GetUserId();
                tokenStore.SetToken(userId!, new DiscordOAuthToken(context));

                await TryAddTeamMemberRoles(context.Principal, oAuth2Api);
            };
        })
        .AddCookie(opts => 
        {
            // Wiggle room for cookie expiration so Blazor authentication state provider
            // can invalidate first
            opts.ExpireTimeSpan = new TimeSpan(7, 0, 5, 0);
        });

        services.AddAuthorization
        (
            opts =>
            {
                opts.AddPolicy(DashboardPolicies.TeamMemberPolicyName,
                               DashboardPolicies.TeamMemberPolicy());

                opts.AddPolicy(DashboardPolicies.ManageGuildPolicyName,
                               DashboardPolicies.ManageGuildPolicy());
            }
        );

        return services;
    }

    private static async Task TryAddTeamMemberRoles
    (
        ClaimsPrincipal       user,
        IDiscordRestOAuth2API oAuth2Api
    )
    {
        try
        {
            var result = await oAuth2Api.GetCurrentBotApplicationInformationAsync();
            if (result.IsDefined(out var appInfo))
            {
                var claimIdentity   = user.Identity as ClaimsIdentity;
                var userIdSnowflake = user.GetUserId().ToSnowflake<Snowflake>();

                if (appInfo.Owner?.ID.Value == userIdSnowflake &&
                    !user.HasRole(DashboardPolicies.BotCreatorRoleName))
                {
                    claimIdentity!.AddClaim(CreateRole(DashboardPolicies.BotCreatorRoleName));
                    claimIdentity!.AddClaim(CreateRole(DashboardPolicies.TeamMemberRoleName));
                }

                var teamMember = appInfo.Team?.Members.FirstOrDefault
                (
                    member => member.User.ID.Value == userIdSnowflake
                );

                if (teamMember is not null &&
                    !user.HasRole(DashboardPolicies.TeamMemberRoleName))
                {
                    claimIdentity!.AddClaim(CreateRole(DashboardPolicies.TeamMemberRoleName));
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private static bool HasRole(this ClaimsPrincipal claimsPrincipal, string roleValue)
        => claimsPrincipal.HasClaim(ClaimTypes.Role, roleValue);

    private static Claim CreateRole(string roleValue) 
        => new(ClaimTypes.Role, roleValue);

    private static IServiceCollection AddSilkDatabase
    (
        this IServiceCollection  services,
        SilkConfigurationOptions silkConfigOptions
    )
    {
        return services.AddDbContextFactory<GuildContext>(dbBuilder =>
        {
            EntityMapping.ConfigureMappings();
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
        services.AddMemoryCache();

        services.AddHttpClient("Github", con => 
        {
            con.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/vnd.github.v3+json");
            con.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", StringConstants.ProjectIdentifier);
        });

        services.AddMudServices(config =>
        {
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomCenter;
        });

        services.AddMediatR(typeof(GuildContext));

        services.AddSingleton<DiscordTokenStore>();
        services.AddScoped<IAuthorizationHandler, ManageGuildAuthorizationHandler>();
        services.AddScoped<AuthenticationStateProvider, DiscordAuthenticationStateProvider>();

        return services;
    }
}