using System.Threading.Tasks;
using AspNet.Security.OAuth.Discord;
using Blazored.Toast;
using DSharpPlus;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
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
            services.AddControllers();
            services.AddServerSideBlazor();

            services.AddBlazoredToast();
            services.AddDataProtection();

            services.AddHttpClient();

            services.AddScoped<IDashboardTokenStorageService, DashboardTokenStorageService>();
            services.AddScoped(provider =>
            {
                var restClient = new DiscordRestClient(new DiscordConfiguration
                {
                    Token = provider.GetRequiredService<IDashboardTokenStorageService>().GetToken()?.AccessToken,
                    TokenType = TokenType.Bearer
                });

                /* Todo: Calls InitializeAsync by default. Let user manually call? */
                restClient.InitializeAsync().GetAwaiter().GetResult();
                return restClient;
            });
            services.AddScoped<DiscordRestClientService>();

            /* Todo: Consolidate Adding SilkConfigurationOptions to common location? */
            services.Configure<SilkConfigurationOptions>(Configuration.GetSection(SilkConfigurationOptions.SectionKey));
            var silkConfig = Configuration.GetSection(SilkConfigurationOptions.SectionKey).Get<SilkConfigurationOptions>();

            services.AddDbContextFactory<GuildContext>(builder =>
            {
                builder.UseNpgsql(silkConfig.Persistence.GetConnectionString());
#if DEBUG
                builder.EnableSensitiveDataLogging();
                builder.EnableDetailedErrors();
#endif
            }, ServiceLifetime.Transient);

            services.TryAdd(new ServiceDescriptor(typeof(GuildContext), p =>
                p.GetRequiredService<IDbContextFactory<GuildContext>>().CreateDbContext(), ServiceLifetime.Transient));

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
                        var tokenStorageService = context.HttpContext.RequestServices.GetRequiredService<IDashboardTokenStorageService>();
                        var tokenExpiration = DiscordOAuthToken.GetAccessTokenExpiration(context.Properties.GetTokenValue("expires_at"));
                        tokenStorageService.SetToken(new DiscordOAuthToken(context.AccessToken, context.RefreshToken, tokenExpiration));
                        return Task.CompletedTask;
                    };

                    opt.Scope.Add("guilds");

                    opt.UsePkce = true;
                    opt.SaveTokens = true;
                })
                .AddCookie(opts => { });
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