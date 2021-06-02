using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Filters;
using Silk.Core.Data;
using Silk.Core.EventHandlers;
using Silk.Core.EventHandlers.Guilds;
using Silk.Core.EventHandlers.MemberAdded;
using Silk.Core.EventHandlers.MemberRemoved;
using Silk.Core.EventHandlers.Messages;
using Silk.Core.EventHandlers.Messages.AutoMod;
using Silk.Core.EventHandlers.Reactions;
using Silk.Core.Services;
using Silk.Core.Services.Interfaces;
using Silk.Core.Utilities;
using Silk.Core.Utilities.Bot;
using Silk.Shared.Constants;

namespace Silk.Core
{
    public class Startup
    {
        public static async Task Main()
        {
            // Make Generic Host here. //
            IHostBuilder? builder = CreateBuilder();

            AddLogging(builder);

            ConfigureServices(builder);
            ConfigureDiscordClient(builder);

            IHost builtBuilder = builder.UseConsoleLifetime().Build();
            DiscordConfigurations.CommandsNext.Services = builtBuilder.Services; // Prevents double initialization of services. //

            await builtBuilder.RunAsync().ConfigureAwait(false);
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "EFCore CLI tools rely on reflection.")]
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return ConfigureServices(CreateBuilder(), false);
        }

        private static IHostBuilder CreateBuilder()
        {
            IHostBuilder? builder = Host.CreateDefaultBuilder();

            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.SetBasePath(Directory.GetCurrentDirectory());
                configuration.AddJsonFile("appSettings.json", true, false);
                configuration.AddUserSecrets<Main>(true, false);
            });
            return builder;
        }

        private static void AddLogging(IHostBuilder host)
        {
            host.ConfigureLogging((builder, _) =>
                {
                    LoggerConfiguration? logger = new LoggerConfiguration()
                        .WriteTo.Console(outputTemplate: StringConstants.LogFormat, theme: SerilogThemes.Bot)
                        .WriteTo.File("./logs/silkLog.log", LogEventLevel.Verbose, StringConstants.LogFormat, retainedFileCountLimit: null)
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .Filter.ByExcluding(Matching.FromSource("DSharpPlus"));

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

        private static IHostBuilder ConfigureServices(IHostBuilder builder, bool addServices = true)
        {
            return builder.ConfigureServices((context, services) =>
            {
                IConfiguration? config = context.Configuration;
                AddDatabases(services, config.GetConnectionString("core"));
                if (!addServices) return;
                services.AddScoped(typeof(ILogger<>), typeof(Shared.Types.Logger<>));

                services.AddSingleton(new DiscordShardedClient(DiscordConfigurations.Discord));

                services.AddMemoryCache(option => option.ExpirationScanFrequency = TimeSpan.FromSeconds(30));

                services.AddHttpClient(StringConstants.HttpClientName, client => client.DefaultRequestHeaders.UserAgent.ParseAdd($"Silk Project by VelvetThePanda / v{StringConstants.Version}"));
                services.AddSingleton(_ => new BotConfig(context.Configuration));

                services.AddSingleton<GuildEventHandlers>();

                services.AddTransient<ConfigService>();
                services.AddSingleton<AntiInviteCore>();
                services.AddTransient<RoleAddedHandler>();
                services.AddTransient<MemberAddedHandler>();
                services.AddTransient<MemberRemovedHandler>();
                services.AddTransient<RoleRemovedHandler>();
                services.AddSingleton<BotExceptionHandler>();
                services.AddSingleton<SerilogLoggerFactory>();
                services.AddTransient<MessageRemovedHandler>();

                services.AddSingleton<CommandHandler>();
                services.AddSingleton<MessageAddAntiInvite>();

                services.AddSingleton<EventHelper>();
                services.AddSingleton<ButtonHandlerService>();

                services.AddScoped<IInputService, InputService>();
                services.AddScoped<IPrefixCacheService, PrefixCacheService>();
                services.AddSingleton<IInfractionService, InfractionService>();
                services.AddSingleton<IServiceCacheUpdaterService, ServiceCacheUpdaterService>();

                services.AddSingleton<TagService>();
                services.AddSingleton<RoleMenuReactionService>();

                //services.AddSingleton<IMessageSender, MessageSenderService>();

                services.AddSingleton<Main>();
                services.AddHostedService(s => s.GetRequiredService<Main>());

                // Couldn't figure out how to get the service since AddHostedService adds it as //
                // IHostedService. Google failed me, but https://stackoverflow.com/a/65552373 helped a lot. //
                services.AddSingleton<ReminderService>();
                services.AddHostedService(b => b.GetRequiredService<ReminderService>());

                services.AddHostedService<StatusService>();

                services.AddMediatR(typeof(Main));
                services.AddMediatR(typeof(GuildContext));

                services.AddSingleton<GuildEventHandlerService>();
                services.AddHostedService(b => b.GetRequiredService<GuildEventHandlerService>());

                services.AddSingleton<UptimeService>();
                //services.AddHostedService(b => b.GetRequiredService<UptimeService>());
            });
        }


        private static void ConfigureDiscordClient(IHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                DiscordConfiguration? client = DiscordConfigurations.Discord;
                IConfiguration? config = context.Configuration;
                int.TryParse(context.Configuration["Shards"] ?? "1", out int shards);

                client.ShardCount = shards;
                client.Token = config.GetConnectionString("discord");
            });
        }

        private static void MigrateDatabases(DbContext[] contexts)
        {
            foreach (var c in contexts)
                c.Database.Migrate();
        }

        private static void AddDatabases(IServiceCollection services, string connectionString)
        {
            void Builder(DbContextOptionsBuilder b)
            {
                b.UseNpgsql(connectionString);
                #if DEBUG
                b.EnableSensitiveDataLogging();
                b.EnableDetailedErrors();
                b.LogTo(Console.WriteLine);
                #endif // EFCore will complain about enabling sensitive data if you're not in a debug build. //
            }

            services.AddDbContext<GuildContext>(Builder, ServiceLifetime.Transient);
            services.AddDbContextFactory<GuildContext>(Builder, ServiceLifetime.Transient);
            services.AddTransient(_ => new DbContextOptionsBuilder<GuildContext>().UseNpgsql(connectionString).Options);
        }

        private void Whatever()
        {
            try
            {
                throw new();
            }
            catch (Exception e) { }
        }

    }
}