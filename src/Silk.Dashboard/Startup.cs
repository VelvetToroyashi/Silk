using System;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Discord;
using DSharpPlus;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using MudBlazor.Services;
using Silk.Core.Data;
using Silk.Dashboard.Models;
using Silk.Dashboard.Services;
using Silk.Shared.Configuration;

namespace Silk.Dashboard
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();

            services.AddControllers();
            services.AddMudServices();

            services.AddHttpClient();

            /* Todo: Consolidate Adding SilkConfigurationOptions to common location? */
            services.Configure<SilkConfigurationOptions>(Configuration.GetSection(SilkConfigurationOptions.SectionKey));
            var silkConfig = Configuration
                .GetSection(SilkConfigurationOptions.SectionKey)
                .Get<SilkConfigurationOptions>();

            services.AddDbContextFactory<GuildContext>(builder =>
            {
                builder.UseNpgsql(silkConfig.Persistence.GetConnectionString());
                #if DEBUG
                builder.EnableSensitiveDataLogging();
                builder.EnableDetailedErrors();
                #endif
            }, ServiceLifetime.Transient);

            services.TryAdd(new ServiceDescriptor(typeof(GuildContext),
                p => p.GetRequiredService<IDbContextFactory<GuildContext>>().CreateDbContext(),
                ServiceLifetime.Transient));

            services.AddSingleton<ITokenService, TokenService>();

            /* Regular DiscordRestClient */
            /* Todo: Handle AccessToken expiration and re-login (singleton won't get updated AccessToken from re-login) */
            services.AddSingleton(provider =>
            {
                var tokenService = provider.GetRequiredService<ITokenService>();
                var restClient = new DiscordRestClient(new DiscordConfiguration
                {
                    Token = tokenService.Token?.AccessToken,
                    TokenType = TokenType.Bearer
                });

                restClient.InitializeAsync().GetAwaiter().GetResult();
                return restClient;
            });

            services.AddScoped<DiscordRestClientService>();

            services.AddMediatR(typeof(GuildContext));

            services.AddAuthentication(opt =>
                {
                    opt.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    opt.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    opt.DefaultChallengeScheme = DiscordAuthenticationDefaults.AuthenticationScheme;
                })
                .AddDiscord(opt =>
                {
                    opt.ClientId = silkConfig.Discord.ClientId;
                    opt.ClientSecret = silkConfig.Discord.ClientSecret;

                    opt.CallbackPath = DiscordAuthenticationDefaults.CallbackPath;

                    opt.Events.OnCreatingTicket = context =>
                    {
                        var tokenService = context.HttpContext.RequestServices.GetRequiredService<ITokenService>();
                        var tokenResponse = new DiscordOAuthTokenResponse(context.AccessToken, context.RefreshToken,
                            DiscordOAuthTokenResponse.GetAccessTokenExpiration(context.Properties.GetTokenValue("expires_at")));
                        tokenService.SetToken(tokenResponse);
                        return Task.CompletedTask;
                    };

                    opt.Scope.Add("guilds");

                    opt.UsePkce = true;
                    opt.SaveTokens = true;
                })
                .AddCookie(options =>
                {
                    /* Set Cookie expiry (default Discord access token expiry time?) */
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapControllers();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}