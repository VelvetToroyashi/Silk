using System;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Silk.Core.Discord.Utilities;
using Silk.Core.Discord.Utilities.Bot;

namespace Silk.Core.Discord
{
    public static class Program
    {
        public static DateTime Startup { get; } = DateTime.Now;
        public static string Version => "1.5.1-alpha";
        public static string HttpClientName { get; } = "Silk";
        private const string LogFormat = "[{Timestamp:h:mm:ss ff tt}] [{Level:u3}] [{SourceContext}] {Message:lj} {Exception:j}{NewLine}";

        private static DiscordConfiguration _clientConfig = new()
        {
            Intents = DiscordIntents.Guilds | // Caching
                      DiscordIntents.GuildMembers | // Auto-mod/Auto-greet
                      DiscordIntents.DirectMessages | // DM Commands
                      DiscordIntents.GuildPresences | // Auto-Mod Anti-Status-Invite
                      DiscordIntents.GuildMessages | // Commands & Auto-Mod
                      DiscordIntents.GuildMessageReactions | // Role-menu
                      DiscordIntents.DirectMessageReactions | // Interactivity in DMs
                      DiscordIntents.GuildVoiceStates,
            LogTimestampFormat = "h:mm:ss ff tt",
            MessageCacheSize = 1024,
            MinimumLogLevel = LogLevel.None,
        };

        // Setting this in the prop doesn't work; it'll have a 2s discrepancy
        //static Program() => Startup = DateTime.Now;

        public static async Task Main(string[] args)
        {
            _ = Startup;

            Console.WriteLine($"Started! The current time is {DateTime.Now:h:mm:ss ff tt}");
            await CreateHostBuilder(args)
                .UseConsoleLifetime()
                .RunConsoleAsync()
                .ConfigureAwait(false);
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseConsoleLifetime()
                .ConfigureAppConfiguration((context, configuration) =>
                {
                    configuration.SetBasePath(Directory.GetCurrentDirectory());
                    configuration.AddJsonFile("appSettings.json", true, false);
                    configuration.AddUserSecrets<Bot>(true, false);
                })
                .ConfigureLogging((builder, _) =>
                {
                    if (int.TryParse(builder.Configuration["Shards"] ?? "1", out int shards))
                        _clientConfig.ShardCount = shards;

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
                    Log.Logger.ForContext(typeof(Program)).Information("Logging initialized!");
                })
                .ConfigureServices((context, services) =>
                {
                    IConfiguration config = context.Configuration;
                    _clientConfig.Token = config.GetConnectionString("botToken");
                    services.AddSingleton(new DiscordShardedClient(_clientConfig));
                    Discord.Startup.AddDatabase(services, config.GetConnectionString("dbConnection"));
                    Discord.Startup.AddServices(services);


                    services.AddMemoryCache(option => option.ExpirationScanFrequency = TimeSpan.FromSeconds(30));

                    services.AddHttpClient(HttpClientName, client =>
                    {
                        client.DefaultRequestHeaders.UserAgent.ParseAdd("Silk Project by VelvetThePanda / v1.4");
                    });

                    // Sub out the default implementation filter with custom filter
                    services.Replace(ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, CustomLoggingFilter>());

                    /* Can remove all filters with this line */
                    // services.RemoveAll<IHttpMessageHandlerBuilderFilter>();

                    services.AddSingleton(_ => new BotConfig(config));


                })
                .UseSerilog();
        }
    }
}