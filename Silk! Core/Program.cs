namespace SilkBot
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NLog.Extensions.Logging;
    using SilkBot.Commands.Bot;
    using SilkBot.Commands.General;
    using SilkBot.Services;
    using SilkBot.Utilities;
    using System;
    using System.Threading.Tasks;
    public class Program
    {
        public static async Task Main()
        {
            //var builder = new ConfigurationBuilder();
            //builder.AddJsonFile("./Silk_Config.JSON");
            //IConfiguration config = builder.Build();
            var bot = new Bot();


            var services = new ServiceCollection()
            .AddMemoryCache(option => option.ExpirationScanFrequency = TimeSpan.FromHours(1))
            .AddSingleton<NLogLoggerFactory>()
            .AddSingleton<PrefixCacheService>()
            .AddSingleton<MessageCreationHandler>()
            .AddSingleton<GuildConfigCacheService>()
            .AddSingleton<TicketService>()
            .AddDbContextFactory<SilkDbContext>(lifetime: ServiceLifetime.Transient)
            .AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.SetMinimumLevel(LogLevel.Trace);
                loggingBuilder.AddNLog("./NLog.config");
            });
            await bot.RunBotAsync((ServiceCollection)services).ConfigureAwait(false);
        }
    } 
}