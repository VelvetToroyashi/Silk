using System;
using DSharpPlus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;
using Silk.Core.Discord.Utilities;
using Silk.Core.Discord.Utilities.Bot;
using Silk.Shared.Constants;

namespace Silk.Core.Discord
{
    public static class Program
    {
        private static readonly DiscordConfiguration _clientConfig = new()
        {
            Intents = FlagConstants.Intents,
            LogTimestampFormat = "h:mm:ss ff tt",
            MessageCacheSize = 1024,
            MinimumLogLevel = LogLevel.None,
            LoggerFactory = new SerilogLoggerFactory()
        };
        public static DateTime Startup { get; } = DateTime.Now;
        public static string HttpClientName { get; } = "Silk";

        // Setting this in the prop doesn't work; it'll have a 2s discrepancy
        //static Program() => Startup = DateTime.Now;

        public static void Start(IHostBuilder host)
        {
            _ = Startup;
            Console.WriteLine($"Started! The current time is {DateTime.Now:h:mm:ss ff tt}");

            ConfigureHost(host);
        }

        private static void ConfigureHost(IHostBuilder host)
        {
            host.ConfigureServices((context, services) =>
            {
                if (int.TryParse(context.Configuration["Shards"] ?? "1", out int shards))
                    _clientConfig.ShardCount = shards;
                IConfiguration config = context.Configuration;
                _clientConfig.Token = config.GetConnectionString("discord");

                services!.AddSingleton(new DiscordShardedClient(_clientConfig));

                Discord.Startup.AddServices(services);

                services.AddMemoryCache(option => option.ExpirationScanFrequency = TimeSpan.FromSeconds(30));

                services.AddHttpClient(HttpClientName, client =>
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Silk Project by VelvetThePanda / v1.5");
                });

                // Sub out the default implementation filter with custom filter
                services.Replace(ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, CustomLoggingFilter>());

                /* Can remove all filters with this line */
                // services.RemoveAll<IHttpMessageHandlerBuilderFilter>();

                services.AddSingleton(_ => new BotConfig(config));
            });
        }
    }
}