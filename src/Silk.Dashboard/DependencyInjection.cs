using System.Security.Claims;
using AspNet.Security.OAuth.Discord;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MudBlazor;
using MudBlazor.Services;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Rest.Extensions;
using Remora.Rest.Core;
using Silk.Dashboard.Extensions;
using Silk.Dashboard.Services.DashboardDiscordClient;
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
        services.AddDiscordRest
        (
         provider => provider.GetRequiredService<IOptions<SilkConfigurationOptions>>()
                             .Value.Discord.BotToken
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
        services.AddAuthentication(opt => 
        {
            opt.DefaultScheme          = CookieAuthenticationDefaults.AuthenticationScheme;
            opt.DefaultSignInScheme    = CookieAuthenticationDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = DiscordAuthenticationDefaults.AuthenticationScheme;
        })
        .AddDiscord(opt => 
        {
            opt.UsePkce    = true;
            opt.SaveTokens = true;

            opt.Scope.Add("guilds");

            opt.ClientId     = silkConfigOptions.Discord.ClientId;
            opt.ClientSecret = silkConfigOptions.Discord.ClientSecret;

            opt.CallbackPath       = DiscordAuthenticationDefaults.CallbackPath;
            opt.AccessDeniedPath   = new("/");
            opt.ReturnUrlParameter = string.Empty;

            opt.Events.OnCreatingTicket = async context =>
            {
                var serviceProvider = context.HttpContext.RequestServices;
                var oAuth2Api       = serviceProvider.GetRequiredService<IDiscordRestOAuth2API>();

                var userId     = context.Principal!.GetUserId();
                var tokenStore = serviceProvider.GetRequiredService<IDiscordTokenStore>();
                tokenStore.SetToken(userId!, new DiscordTokenStoreEntry(context));

                await TryAddTeamMemberRoles(context.Principal, oAuth2Api);
            };
        })
        .AddCookie(options => 
        {
            // Todo: Find way to set expiration based on OAuth token expiry
            options.ExpireTimeSpan = TimeSpan.FromDays(7);
        });

        services.AddAuthorization
        (
            options => options.AddPolicy(DashboardPolicies.TeamMemberPolicyName, 
                                         DashboardPolicies.TeamMemberPolicy())
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
        services.AddHttpContextAccessor();

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

        /* Todo: Handle Logout of User when expired token belongs to current user */
        /* Todo: Handle Removal of expired Cookies */
        services.AddSingleton<IDiscordTokenStore, DiscordTokenStore>();
        services.AddSingleton<IDiscordTokenStoreWatcher, DiscordTokenStoreWatcher>();
        services.AddHostedService(s => s.GetRequiredService<IDiscordTokenStoreWatcher>());

        return services;
    }
}