using System;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Silk.Core.Utilities;
using Silk.Core.Utilities.Bot;
using Silk.Data;
using Silk.Data.MediatR;

namespace Silk.Core
{
    public class Program
    {
        public static DateTime Startup { get; } = DateTime.Now;


        public static string HttpClientName { get; } = "Silk";

        private static readonly DiscordConfiguration _clientConfig = new()
        {
            Intents = DiscordIntents.Guilds | // Caching
                      DiscordIntents.GuildMembers | //Auto-mod/Auto-greet
                      DiscordIntents.DirectMessages | // Commands & Auto-Mod
                      DiscordIntents.GuildPresences | // Role-menu
                      DiscordIntents.GuildMessages | // DM Commands
                      DiscordIntents.GuildMessageReactions |
                      DiscordIntents.DirectMessageReactions, // Auto-mod,
            MessageCacheSize = 1024,
            MinimumLogLevel = LogLevel.None
        };

        public static async Task Main(string[] args) =>
            await CreateHostBuilder(args)
                .UseConsoleLifetime()
                .StartAsync()
                .ConfigureAwait(false);


        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseConsoleLifetime()
                .ConfigureAppConfiguration((_, configuration) =>
                {
                    configuration.SetBasePath(Directory.GetCurrentDirectory());
                    configuration.AddJsonFile("appSettings.json", true, false);
                    configuration.AddUserSecrets<Program>(true, false);
                })
                .ConfigureLogging((builder, _) =>
                {
                    const string logFormat = "[{Timestamp:h:mm:ss ff tt}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";
                    var logger = new LoggerConfiguration()
                        .WriteTo.Console(outputTemplate: logFormat, theme: SerilogThemes.Bot)
                        .WriteTo.File("./logs/silkLog.log", LogEventLevel.Verbose, logFormat, rollingInterval: RollingInterval.Day, retainedFileCountLimit: null)
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
                })
                .ConfigureServices((context, services) =>
                {
                    IConfiguration config = context.Configuration;
                    _clientConfig.Token = config.GetConnectionString("BotToken");
                    services.AddSingleton(new DiscordShardedClient(_clientConfig));
                    Core.Startup.AddDatabase(services, config.GetConnectionString("dbConnection"));
                    Core.Startup.AddServices(services);


                    services.AddMemoryCache(option => option.ExpirationScanFrequency = TimeSpan.FromSeconds(30));

                    services.AddHttpClient(HttpClientName, client =>
                    {
                        client.DefaultRequestHeaders.UserAgent.ParseAdd("Silk Project by VelvetThePanda / v1.4");
                    });

                    // Sub out the default implementation filter with custom filter
                    services.Replace(ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, CustomLoggingFilter>());

                    /* Can remove all filters with this line */
                    // services.RemoveAll<IHttpMessageHandlerBuilderFilter>();

                    services.AddTransient(_ => new BotConfig(config));

                    services.AddHostedService<Bot>();
                    services.AddMediatR(typeof(Program));
                    services.AddMediatR(typeof(GuildRequest));
                })
                .UseSerilog();
        }
    }
}