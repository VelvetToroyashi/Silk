namespace SilkBot
{
    using DSharpPlus;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Serilog;
    using Serilog.Extensions.Logging;
    using Serilog.Sinks.SystemConsole.Themes;
    using SilkBot.Commands.Bot;
    using SilkBot.Commands.General;
    using SilkBot.Services;
    using SilkBot.Tools;
    using SilkBot.Utilities;
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using LogLevel = Microsoft.Extensions.Logging.LogLevel;


    public class Program
    {
        private static readonly DiscordConfiguration clientConfig = new DiscordConfiguration
        {
            Intents = DiscordIntents.All,
            MessageCacheSize = 4096,
            MinimumLogLevel = LogLevel.None
        };

        public static async Task Main(string[] args) => await CreateHostBuilder(args).RunConsoleAsync().ConfigureAwait(false);


        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseConsoleLifetime()
            .ConfigureAppConfiguration((context, configuration) =>
            {
                configuration.SetBasePath(Directory.GetCurrentDirectory());
                configuration.AddJsonFile("appSettings.json", false, false);
            })
            .ConfigureLogging((context, builder) => Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "[{Timestamp:h:mm:ss-ff tt}] [{Level:u3}] {Message:lj}{NewLine}{Exception}", theme: SerilogThemes.Bot)
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Verbose()
            .CreateLogger())
            .ConfigureServices((context, services) =>
            {
                IConfiguration config = context.Configuration;
                clientConfig.Token = config.GetConnectionString("BotToken");
                services.AddSingleton(new DiscordShardedClient(clientConfig));
                services.AddDbContextFactory<SilkDbContext>(option => option.UseNpgsql(config.GetConnectionString("dbConnection")), ServiceLifetime.Transient);
                services.AddMemoryCache(option => option.ExpirationScanFrequency = TimeSpan.FromHours(1));

                services.AddSingleton<PrefixCacheService>();
                services.AddSingleton<GuildConfigCacheService>();
                services.AddSingleton<SerilogLoggerFactory>();
                services.AddSingleton<TicketService>();
                services.AddSingleton<MessageCreationHandler>();
                services.AddSingleton<TimedEventService>();
                services.AddSingleton(typeof(HttpClient), (services) =>
                {
                    var client = new HttpClient();
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Silk Project by VelvetThePanda / v1.3");
                    return client;
                });
                services.AddSingleton<BotEventHelper>();
                
                services.AddHostedService<Bot>();
            })
            .UseSerilog();
    }
}