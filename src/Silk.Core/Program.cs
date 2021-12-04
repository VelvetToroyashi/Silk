using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Remora.Commands.Extensions;
using Remora.Discord.Gateway.Extensions;
using Remora.Discord.Gateway.Responders;
using Serilog;
using Silk.Core.Commands.Conditions.cs;
using Silk.Core.Data;
using Silk.Core.Responders;
using Silk.Core.Services.Data;
using Silk.Core.Services.Interfaces;
using Silk.Shared.Configuration;

namespace Silk.Core
{
    public class Program
    {
        public static async Task Main()
        {
            IHostBuilder? host = Host
                                .CreateDefaultBuilder()
                                .UseConsoleLifetime();

            host.ConfigureAppConfiguration(configuration =>
            {
                configuration.SetBasePath(Directory.GetCurrentDirectory());
                configuration.AddJsonFile("appSettings.json", true, false);
                configuration.AddUserSecrets("VelvetThePanda-Silk", false);
            });

            ConfigureServices(host);

            await host.Build().RunAsync();
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "EFCore CLI tools rely on reflection.")]
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            IHostBuilder? builder = Host
               .CreateDefaultBuilder(args);

            builder.ConfigureServices((context, container) =>
            {
                SilkConfigurationOptions? silkConfig = context.Configuration.GetSilkConfigurationOptionsFromSection();
                AddDatabases(container, silkConfig.Persistence);
            });

            return builder;
        }

        private static IHostBuilder ConfigureServices(IHostBuilder builder)
        {
            builder
               .ConfigureLogging(l => l.ClearProviders().AddSerilog())
               .ConfigureServices((context, services) =>
                {
                    // There's a more elegant way to do this, but I'm lazy and this works.
                    SilkConfigurationOptions? silkConfig = context.Configuration.GetSilkConfigurationOptionsFromSection();

                    AddSilkConfigurationOptions(services, context.Configuration);
                    AddDatabases(services, silkConfig.Persistence);

                    services.AddSilkLogging(context);

                    services.AddResponder<EarlyCacheSnapshotResponder>(ResponderGroup.Early);
                    services.AddResponder<LateCacheSnapshotRespodner>(ResponderGroup.Late);
                    
                    services.AddCondition<RequireNSFWCondition>();
                    
                    services.AddSingleton<IPrefixCacheService, PrefixCacheService>();
                    services.AddSingleton<GuildCacherService>();
                    //services.AddScoped<SilkCommandResponder>(); // So Remora's default responder can be overridden. I'll remove this when my PR is merged. //
                    services.AddRemoraServices();


                    services.AddMediatR(typeof(Program));
                    services.AddMediatR(typeof(GuildContext));
                })
               .AddRemoraHosting();

            return builder;
        }



        private static void AddSilkConfigurationOptions(IServiceCollection services, IConfiguration configuration)
        {
            // Add and Bind IOptions configuration for appSettings.json and UserSecrets configuration structure
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-5.0
            IConfigurationSection? silkConfigurationSection = configuration.GetSection(SilkConfigurationOptions.SectionKey);
            services.Configure<SilkConfigurationOptions>(silkConfigurationSection);
        }

        private static void AddDatabases(IServiceCollection services, SilkPersistenceOptions persistenceOptions)
        {
            void Builder(DbContextOptionsBuilder b)
            {
                b.UseNpgsql(persistenceOptions.GetConnectionString());
                #if DEBUG
                b.EnableSensitiveDataLogging();
                b.EnableDetailedErrors();
                #endif // EFCore will complain about enabling sensitive data if you're not in a debug build. //
            }

            services.AddDbContext<GuildContext>(Builder, ServiceLifetime.Transient);
            services.AddDbContextFactory<GuildContext>(Builder, ServiceLifetime.Transient);
            //services.TryAdd(new ServiceDescriptor(typeof(GuildContext), p => p.GetRequiredService<IDbContextFactory<GuildContext>>().CreateDbContext(), ServiceLifetime.Transient));
        }
    }

    //Todo: Move this class maybe? 
    public static class IConfigurationExtensions
    {
	    /// <summary>
	    ///     An extension method to get a <see cref="SilkConfigurationOptions" /> instance from the Configuration by Section Key
	    /// </summary>
	    /// <param name="config">the configuration</param>
	    /// <returns>an instance of the SilkConfigurationOptions class, or null if not found</returns>
	    public static SilkConfigurationOptions GetSilkConfigurationOptionsFromSection(this IConfiguration config)
        {
            return config.GetSection(SilkConfigurationOptions.SectionKey).Get<SilkConfigurationOptions>();
        }
    }
}