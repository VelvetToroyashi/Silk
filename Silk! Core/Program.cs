namespace SilkBot
{
    using DSharpPlus;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Serilog;
    using Serilog.Core;
    using Serilog.Extensions.Logging;
    using SilkBot.Commands.General;
    using SilkBot.Extensions;
    using SilkBot.Services;
    using SilkBot.Tools;
    using SilkBot.Utilities;
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;
    using LogLevel = Microsoft.Extensions.Logging.LogLevel;

    public class Program
    {
        private static readonly DiscordConfiguration clientConfig = new DiscordConfiguration
        {
            Intents = DiscordIntents.All,
            LogTimestampFormat = "H:mm:sstt",
            MessageCacheSize = 4096,
            MinimumLogLevel = LogLevel.None
        };

        public static async Task Main(string[] args) => CreateHostBuilder(args).RunConsoleAsync().GetAwaiter();
            

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseConsoleLifetime()
            .ConfigureAppConfiguration((context, configuration) =>
            {
                configuration.SetBasePath(Directory.GetCurrentDirectory());
                configuration.AddJsonFile("appSettings.json", false, false);
            })
            .ConfigureLogging((context, builder) =>
            {
                Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(context.Configuration).CreateLogger();

            })
            .ConfigureServices((context, services) =>
            {
                IConfiguration config = context.Configuration;
                clientConfig.Token = config.GetConnectionString("BotToken");
                services.AddSingleton(new DiscordShardedClient(clientConfig));
                services.AddDbContextFactory<SilkDbContext>(options =>
                {
                    options.UseNpgsql(config.GetConnectionString("dbConnection"));
                    options.UseLoggerFactory(null);
                }, ServiceLifetime.Transient);
                services.AddMemoryCache(option => option.ExpirationScanFrequency = TimeSpan.FromHours(1));

                services.AddSingleton<PrefixCacheService>();
                services.AddSingleton<GuildConfigCacheService>();
                services.AddSingleton<SerilogLoggerFactory>();
                services.AddSingleton<TicketService>();
                services.AddSingleton<TimedEventService>();
                services.AddSingleton(typeof(HttpClient), (services) =>
                {
                    var client = new HttpClient();
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Silk Project by VelvetThePanda / v1.3");
                    return client;
                });
                services.AddSingleton(typeof(BotEventHelper), (services) => 
                {
                    var eHelper = new BotEventHelper(services.Get<DiscordShardedClient>(), services.Get<IDbContextFactory<SilkDbContext>>(), services.Get<ILogger<BotEventHelper>>());
                    eHelper.CreateHandlers();
                    return eHelper;
                });
                
                services.AddHostedService<Bot>();
            }).UseSerilog();
    }
}