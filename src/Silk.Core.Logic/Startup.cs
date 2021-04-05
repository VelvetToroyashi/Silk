using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Silk.Core.Data;
using Silk.Core.Discord;
using Silk.Core.Discord.Utilities;

namespace Silk.Core.Logic
{
    public class Startup
    {
        private const string LogFormat = "[{Timestamp:h:mm:ss ff tt}] [{Level:u3}] [{SourceContext}] {Message:lj} {Exception:j}{NewLine}";

        public static async Task Main()
        {
            // Make Generic Host here. //
            var builder = CreateHostBuilder();
            AddLogging(builder);
            ConfigureServices(builder);
            Program.Start(builder);

            builder.UseConsoleLifetime();

            await builder.RunConsoleAsync().ConfigureAwait(false);
        }

        private static IHostBuilder CreateHostBuilder()
        {
            var builder = Host.CreateDefaultBuilder();

            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.SetBasePath(Directory.GetCurrentDirectory());
                configuration.AddJsonFile("appSettings.json", true, false);
                configuration.AddUserSecrets<Startup>(true, false);
            });
            return builder;
        }

        private static void AddLogging(IHostBuilder host)
        {
            host.ConfigureLogging((builder, _) =>
            {
                var logger = new LoggerConfiguration()
                    .WriteTo.Console(outputTemplate: LogFormat, theme: SerilogThemes.Bot)
                    .WriteTo.File("./logs/silkLog.log", LogEventLevel.Verbose, LogFormat, rollingInterval: RollingInterval.Day, retainedFileCountLimit: null)
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Error);

                Log.Logger = builder.Configuration["LogLevel"] switch
                {
                    "All" => logger.MinimumLevel.Verbose().CreateLogger(),
                    "Info" => logger.MinimumLevel.Information().CreateLogger(),
                    "Debug" => logger.MinimumLevel.Debug().CreateLogger(),
                    "Warning" => logger.MinimumLevel.Warning().CreateLogger(),
                    "Error" => logger.MinimumLevel.Error().CreateLogger(),
                    "Panic" => logger.MinimumLevel.Fatal().CreateLogger(),
                    _ => logger.MinimumLevel.Information().CreateLogger()
                };
                Log.Logger.ForContext(typeof(Startup)).Information("[BACKEND] Logging initialized!");
            });
        }

        private static void ConfigureServices(IHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                var config = context.Configuration;
                AddDatabases(services, config.GetConnectionString("core"));
            });
        }

        private static void AddDatabases(IServiceCollection services, string connectionString)
        {
            services.AddDbContextFactory<GuildContext>(
                option =>
                {
                    option.UseNpgsql(connectionString);
                    #if DEBUG
                    option.EnableSensitiveDataLogging();
                    option.EnableDetailedErrors();
                    #endif // EFCore will complain about enabling sensitive data if you're not in a debug build. //
                }, ServiceLifetime.Transient);
        }
    }
}