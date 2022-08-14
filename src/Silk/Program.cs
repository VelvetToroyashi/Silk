using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NodaTime;
using NodaTime.TimeZones;
using Prometheus;
using Remora.Commands.Extensions;
using Remora.Discord.API.Gateway.Commands;
using Remora.Discord.Caching.Redis.Extensions;
using Remora.Discord.Gateway;
using Remora.Results;
using Sentry;
using Sentry.Extensions.Logging;
using Sentry.Extensions.Logging.Extensions.DependencyInjection;
using Serilog;
using Silk.Commands.Conditions;
using Silk.Data;
using Silk.Data.MediatR.Users.History;
using Silk.Services.Bot;
using Silk.Services.Data;
using Silk.Services.Guild;
using Silk.Services.Interfaces;
using Silk.Shared.Configuration;
using Silk.Shared.Constants;
using Silk.Utilities;
using StackExchange.Redis;

namespace Silk;

public class Program
{
    public static async Task Main()
    {
        
        
        Console.WriteLine("Starting Silk!...");

        IHostBuilder? hostBuilder = Host
                                   .CreateDefaultBuilder()
                                   .UseConsoleLifetime();

        ConfigureApp(hostBuilder);
        ConfigureServices(hostBuilder).AddPlugins();
        AddRedisAndAcquireShard(hostBuilder);

        Console.WriteLine("Configured services.");

        IHost? host = hostBuilder.Build();

        Console.WriteLine("Host is built.");

        Log.ForContext<Program>().Information("Attempting to migrate core database");
        var coreMigrationResult = await EnsureDatabaseCreatedAndApplyMigrations(host);

        if (coreMigrationResult.IsDefined(out var migrationsApplied))
        {
            Log.ForContext<Program>().Information(migrationsApplied > 0 
                                ? "Successfully applied migrations to core database." 
                                : "No pending migrations to apply to core database.");
        }
        else
        {
            Log.ForContext<Program>().Fatal("Failed to migrate core database. Error: {Error}", coreMigrationResult.Error);
            return;
        }

        Log.ForContext<Program>().Information("Startup checks OK. Starting Silk!");

        var metrics = new KestrelMetricServer(6000);

        try { metrics.Start(); } catch { /* ignored */ }

        await host.RunAsync();

        await metrics.StopAsync();
    }

    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "EFCore CLI tools rely on reflection.")]
    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        IHostBuilder? builder = Host.CreateDefaultBuilder(args);
        builder.ConfigureServices((context, services) => services.AddSilkDatabase(context.Configuration));
        return builder;
    }

    private static void ConfigureApp(IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureAppConfiguration
        (
             builder =>
             {
                 // Need to specify configuration providers explicitly
                 // due to custom 'appSettings.json' file, as well as
                 // UserSecrets only being available by default in Development mode
                 builder.AddJsonFile("appSettings.json", optional: true);
                 builder.AddUserSecrets<Program>(optional: true);
                 builder.AddEnvironmentVariables("SILK_");
             }
        );
    }

    private static void AddRedisAndAcquireShard(IHostBuilder builder)
    {
        builder.ConfigureServices
        (
             (context, services) =>
             {
                 Log.ForContext<Program>().Information("Attempting to acquire shard ID from Redis...");

                 var configOptions = context.Configuration.GetSilkConfigurationOptions();
                 var redisConfig   = configOptions.Redis;

                 var connectionConfig = new ConfigurationOptions
                 {
                     EndPoints          = { { redisConfig.Host, redisConfig.Port } },
                     Password           = redisConfig.Password,
                     DefaultDatabase    = redisConfig.Database,
                     SyncTimeout        = 90000,
                     AbortOnConnectFail = false
                 };

                var redis = ConnectionMultiplexer.Connect(connectionConfig);

                var db    = redis.GetDatabase();
                var taken = false;

                var takenShard = 0;

                while (true)
                {
                    for (int i = 0; i < configOptions.Discord.Shards; i++)
                    {
                        var key = $"shard:{i}";

                        if (db.KeyExists(key))
                            continue;

                        db.StringSet(key, "", TimeSpan.FromSeconds(7));
                        Metrics.DefaultRegistry.SetStaticLabels(new() { { "shard", i.ToString() } });

                        takenShard = i;

                        taken = true;
                        break;
                    }

                    if (taken) break;

                    Thread.Sleep(1000);
                }

                 Log.ForContext<Program>().Information("Acquired shard ID {Shard}", takenShard);

                 services.AddSingleton<IConnectionMultiplexer>(redis);
                 services.AddDiscordRedisCaching(r => r.ConfigurationOptions = connectionConfig);

                 services.Configure<DiscordGatewayClientOptions>
                 (
                    gw => gw.ShardIdentification = new ShardIdentification(takenShard, configOptions.Discord.Shards)
                 );
             }
        );
    }

    private static async Task<Result<int>> EnsureDatabaseCreatedAndApplyMigrations(IHost builtBuilder)
    {
        try
        {
            using var serviceScope = builtBuilder.Services.CreateScope();

            await using var dbContext = serviceScope
                                       .ServiceProvider
                                       .GetRequiredService<GuildContext>();

            var pendingMigrations = (await dbContext.Database
                                                   .GetPendingMigrationsAsync()).ToList();

            if (pendingMigrations.Any())
                await dbContext.Database.MigrateAsync();

            return Result<int>.FromSuccess(pendingMigrations.Count);
        }
        catch (Exception e)
        {
            return Result<int>.FromError(new ExceptionError(e));
        }
    }

    private static IHostBuilder ConfigureServices(IHostBuilder builder)
    {
        builder
           .ConfigureLogging(l => l.ClearProviders().AddSerilog())
           .ConfigureServices((context, services) =>
            {
                var silkConfig = context.Configuration.GetSilkConfigurationOptions();

                // A little note on Sentry; it's important to initialize logging FIRST
                // And then sentry, because we set the settings for sentry later. 
                // If we configure logging after, it'll override the settings with defaults.

                services
                   .AddSilkConfigurationOptions(context.Configuration)
                   .AddSilkDatabase(context.Configuration)
                   .AddSilkLogging(context.Configuration)
                   .AddRemoraServices()
                   .AddSingleton<ShardHelper>()
                   .AddSingleton<ReminderService>()
                   .AddHostedService(s => s.GetRequiredService<ReminderService>())
                   .AddSingleton<PhishingGatewayService>()
                   .AddHostedService(s => s.GetRequiredService<PhishingGatewayService>())
                   .AddSingleton<PhishingDetectionService>()
                   .AddCondition<RequireNSFWCondition>()
                   .AddCondition<RequireTeamOrOwnerCondition>()
                   .AddSingleton<MemberScannerService>()
                   .AddSingleton<IPrefixCacheService, PrefixCacheService>()
                   .AddSingleton<IInfractionService, InfractionService>()
                   .AddHostedService(s => (s.GetRequiredService<IInfractionService>() as InfractionService)!)
                   .AddSingleton<InviteDetectionService>()
                   .AddSingleton<ExemptionEvaluationService>()
                   .AddSingleton<IChannelLoggingService, ChannelLoggingService>()
                   .AddSingleton<MemberLoggerService>()
                   .AddSingleton<GuildConfigCacheService>()
                   .AddScoped<GuildCacherService>()
                   .AddSingleton<IClock>(SystemClock.Instance)
                   .AddSingleton<IDateTimeZoneSource>(TzdbDateTimeZoneSource.Default)
                   .AddSingleton<IDateTimeZoneProvider, DateTimeZoneCache>()
                   .AddTransient<TimeHelper>()
                   .AddSingleton<GuildGreetingService>()
                   .AddHostedService(s => s.GetRequiredService<GuildGreetingService>())
                   .AddSingleton<FlagOverlayService>()
                   .AddSingleton<RaidDetectionService>()
                   .AddHostedService(s => s.GetRequiredService<RaidDetectionService>())
                   .AddSingleton<MessageLoggerService>()
                   .AddMediatR(c => c.AsTransient(), typeof(Program).Assembly, typeof(GuildContext).Assembly)
                    // Very high throughput handler that needs to be explicitly disposed of,
                    // else it'll gobble up connections.
                   .AddScoped(typeof(AddUserJoinDate).GetNestedTypes(BindingFlags.NonPublic)[0])
                   .AddScoped(typeof(AddUserLeaveDate).GetNestedTypes(BindingFlags.NonPublic)[0])
                   .AddSentry<SentryLoggingOptions>()
                   .Configure<SentryLoggingOptions>
                    (
                     slo =>
                     {
                         slo.Dsn                    = silkConfig.SentryDSN;
                         slo.InitializeSdk          = !silkConfig.SelfHosted;
                         slo.MinimumEventLevel      = LogLevel.Error;
                         slo.MinimumBreadcrumbLevel = LogLevel.Trace;
                         slo.Release                = StringConstants.Version;
                         slo.DeduplicateMode        = DeduplicateMode.SameExceptionInstance;
                         slo.TracesSampleRate       = 1.0;
                         slo.AddDiagnosticSourceIntegration();
                     }
                    )
                   .AddHttpClient
                   (
                    "ravy-api",
                    (s, c) =>
                    {
                        var config = s.GetRequiredService<IConfiguration>().GetSilkConfigurationOptions();

                        c.BaseAddress = new("https://ravy.org/api/v1/avatars");
                        c.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Ravy {config.RavyAPIKey}");
                        c.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", StringConstants.ProjectIdentifier);
                    }
                   );
            });

        return builder;
    }
}