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
using Silk.Core.Data;
using Silk.Core.Discord;
using Silk.Core.Discord.EventHandlers;
using Silk.Core.Discord.EventHandlers.Guilds;
using Silk.Core.Discord.EventHandlers.MemberAdded;
using Silk.Core.Discord.EventHandlers.MessageAdded.AutoMod;
using Silk.Core.Discord.Services;
using Silk.Core.Discord.Services.Interfaces;
using Silk.Core.Discord.Utilities;
using Silk.Core.Discord.Utilities.Bot;
using Silk.Shared.Abstractions.DSharpPlus.Interfaces;
using Silk.Shared.Constants;

namespace Silk.Core.Logic
{
    public class Startup
    {
        public static DateTime StartupTime { get; } = DateTime.Now;

        private static ILogger<Startup> _logger;

        public static async Task Main()
        {
            _ = StartupTime; // Properties 
            // Make Generic Host here. //
            var builder = CreateBuilder();

            AddLogging(builder);

            ConfigureServices(builder);
            ConfigureDiscordClient(builder);

            await builder
                .UseConsoleLifetime()
                .RunConsoleAsync()
                .ConfigureAwait(false);
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "EFCore CLI tools rely on reflection.")]
        public static IHostBuilder CreateHostBuilder(string[] args) => ConfigureServices(CreateBuilder());

        private static IHostBuilder CreateBuilder()
        {
            var builder = Host.CreateDefaultBuilder();

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
                    var logger = new LoggerConfiguration()
                        .WriteTo.Console(outputTemplate: StringConstants.LogFormat, theme: SerilogThemes.Bot)
                        .WriteTo.File("./logs/silkLog.log", LogEventLevel.Verbose, StringConstants.LogFormat, rollingInterval: RollingInterval.Day, retainedFileCountLimit: null)
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .MinimumLevel.Override("DSharpPlus", LogEventLevel.Fatal);

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

                services.AddSingleton(new DiscordShardedClient(DiscordConfigurations.Discord));

                services.AddMemoryCache(option => option.ExpirationScanFrequency = TimeSpan.FromSeconds(30));

                services.AddHttpClient(StringConstants.HttpClientName, client => client.DefaultRequestHeaders.UserAgent.ParseAdd($"Silk Project by VelvetThePanda / v{StringConstants.Version}"));
                services.AddSingleton(_ => new BotConfig(context.Configuration));


                services.AddTransient<ConfigService>();
                services.AddTransient<GuildContext>();
                services.AddSingleton<AntiInviteCore>();
                services.AddTransient<RoleAddedHandler>();
                services.AddTransient<MemberAddedHandler>();
                services.AddTransient<RoleRemovedHandler>();
                services.AddSingleton<BotExceptionHandler>();
                services.AddSingleton<SerilogLoggerFactory>();
                services.AddTransient<MessageRemovedHandler>();

                services.AddScoped<IInputService, InputService>();

                services.AddScoped<IInfractionService, InfractionService>();
                services.AddTransient<IPrefixCacheService, PrefixCacheService>();
                services.AddSingleton<IServiceCacheUpdaterService, ServiceCacheUpdaterService>();

                services.AddSingleton<TagService>();

                services.AddSingleton<IMessageSender, MessageSenderService>();

                services.AddSingleton<Main>();
                services.AddHostedService(s => s.GetRequiredService<Main>());

                //Copped this hack from: https://stackoverflow.com/a/65552373 //
                services.AddSingleton<ReminderService>();
                services.AddHostedService(b => b.GetRequiredService<ReminderService>());

                services.AddHostedService<StatusService>();

                services.AddMediatR(typeof(Main));
                services.AddMediatR(typeof(GuildContext));

                services.AddSingleton<GuildEventHandlerService>();
                services.AddHostedService(b => b.GetRequiredService<GuildEventHandlerService>());
            });
        }


        private static void ConfigureDiscordClient(IHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                var client = DiscordConfigurations.Discord;
                var config = context.Configuration;
                int.TryParse(context.Configuration["Shards"] ?? "1", out int shards);

                client.ShardCount = shards;
                client.Token = config.GetConnectionString("discord");

                DiscordConfigurations.CommandsNext.Services = services.BuildServiceProvider();
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
    }
}