using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Silk.Core.Data;
using Silk.Core.Discord;
using Silk.Core.Discord.Utilities;

namespace Silk.Core.Logic
{
    public class Startup
    {
        private const string LogFormat = "[{Timestamp:h:mm:ss ff tt}] [{Level:u3}] [{SourceContext:l}] {Message:lj} {Exception:j}{NewLine}";

        public static async Task Main()
        {
            // Make Generic Host here. //
            var builder = CreateBuilder();
            ConfigureServices(builder);
            AddLogging(builder);
            Program.Start(builder);

            builder.UseConsoleLifetime();

            await builder.RunConsoleAsync().ConfigureAwait(false);

        }
        // EFCore calls this. //
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return ConfigureServices(CreateBuilder());
        }

        private static IHostBuilder CreateBuilder()
        {
            var builder = Host.CreateDefaultBuilder();

            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.SetBasePath(Directory.GetCurrentDirectory());
                configuration.AddJsonFile("appSettings.json", true, false);
                configuration.AddUserSecrets<Discord.Startup>(true, false);
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
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                        .MinimumLevel.Override("DSharpPlus", LogEventLevel.Warning);

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
                    Log.Logger.ForContext(typeof(Startup)).Information("Logging initialized!");
                })
                .UseSerilog();
        }

        private static IHostBuilder ConfigureServices(IHostBuilder builder)
        {
            return builder.ConfigureServices((context, services) =>
            {
                var config = context.Configuration;
                AddDatabases(services, config.GetConnectionString("core"));
                services.AddScoped(typeof(ILogger<>), typeof(Shared.Types.Logger<>));
            });
        }

        private static void AddDatabases(IServiceCollection services, string connectionString)
        {
            void Builder(DbContextOptionsBuilder b)
            {
                b.UseNpgsql(connectionString);
                #if DEBUG
                b.EnableSensitiveDataLogging();
                b.EnableDetailedErrors();
                #endif // EFCore will complain about enabling sensitive data if you're not in a debug build. //
            }

            services.AddDbContext<GuildContext>(Builder, ServiceLifetime.Transient);
            services.AddDbContextFactory<GuildContext>(Builder, ServiceLifetime.Transient);
            using var scope = services.BuildServiceProvider().CreateScope();
            services.AddTransient(_ => new DbContextOptionsBuilder<GuildContext>().UseNpgsql(connectionString).Options);
        }
    }
}