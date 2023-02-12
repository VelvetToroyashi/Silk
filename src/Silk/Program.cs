using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
using Serilog.Templates;
using Silk.Commands.Conditions;
using Silk.Data;
using Silk.Services.Bot;
using Silk.Services.Data;
using Silk.Services.Guild;
using Silk.Services.Interfaces;
using Silk.Shared;
using Silk.Shared.Configuration;
using Silk.Shared.Constants;
using Silk.Utilities;
using StackExchange.Redis;

namespace Silk;

public class Program
{
    public static async Task Main()
    {
        
        IHostBuilder? hostBuilder = Host
                                   .CreateDefaultBuilder()
                                   .UseConsoleLifetime();

        ConfigureApp(hostBuilder);
        ConfigureServices(hostBuilder).AddPlugins();

        Log.ForContext<Program>().Information("Configured services.");

        IHost? host = hostBuilder.Build();

        Log.ForContext<Program>().Information("Host is built.");

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
                 
                 Log.Logger = new LoggerConfiguration()
                             .Enrich.WithProperty("Shard", "?")
                             .Enrich.WithProperty("Shards", builder.Build()["silk:discord:shards"])
                             .WriteTo.Console(new ExpressionTemplate(StringConstants.LogFormat, theme: SilkLogTheme.TemplateTheme))
                             .CreateLogger();
             }
        );
        
    }

    private static void AddRedisAndAcquireShard(IHostBuilder builder, out int takenShard)
    {
        var t = takenShard = 0;
        
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
                

                while (true)
                {
                    for (int i = 0; i < configOptions.Discord.Shards; i++)
                    {
                        var key = $"shard:{i}";

                        if (db.KeyExists(key))
                            continue;

                        db.StringSet(key, "", TimeSpan.FromSeconds(7));
                        Metrics.DefaultRegistry.SetStaticLabels(new Dictionary<string, string>{ { "shard", i.ToString() } });

                        t = i;

                        taken = true;
                        break;
                    }

                    if (taken) break;

                    Thread.Sleep(1000);
                }
                
                 Log.ForContext<Program>().Information("Acquired shard ID {Shard}", t);

                 services.AddSingleton<IConnectionMultiplexer>(redis);
                 services.AddDiscordRedisCaching(r => r.ConfigurationOptions = connectionConfig);

                 services.Configure<DiscordGatewayClientOptions>
                 (
                    gw => gw.ShardIdentification = new ShardIdentification(t, configOptions.Discord.Shards)
                 );
             }
        );
        
        takenShard = t;
    }

    private static async Task<Result<int>> EnsureDatabaseCreatedAndApplyMigrations(IHost builtBuilder)
    {
        try
        {
                                                    using var serviceScope = builtBuilder.Services.CreateScope();

                                                    await using var dbContext = await serviceScope
                                                                               .ServiceProvider
                                                                               .GetRequiredService<IDbContextFactory<GuildContext>>().CreateDbContextAsync();

                                                    //var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync()).ToList();

            //if (pendingMigrations.Any())
                await dbContext.Database.MigrateAsync();

            return Result<int>.FromSuccess(1);
        }
        catch (Exception e)
        {
            return Result<int>.FromError(new ExceptionError(e));
        }
    }

    private static IHostBuilder ConfigureServices(IHostBuilder builder)
    {
        builder.ConfigureServices(se => se.AddRemoraServices());

        AddRedisAndAcquireShard(builder, out var shardId);
        
        builder
           .ConfigureServices((context, services) =>
            {
                var silkConfig = context.Configuration.GetSilkConfigurationOptions();

                // A little note on Sentry; it's important to initialize logging FIRST
                // And then sentry, because we set the settings for sentry later. 
                // If we configure logging after, it'll override the settings with defaults.

                services
                   .AddMediator()
                   //.AddSingleton<ScopeWrapper>()
                   .AddSilkConfigurationOptions(context.Configuration)
                   .AddSilkLogging(context.Configuration, shardId)
                   .AddSilkDatabase(context.Configuration)
                   //.AddRemoraServices()
                   .AddSingleton<ShardHelper>()
                   .AddSingleton<ReminderService>()
                   .AddHostedService(s => s.GetRequiredService<ReminderService>())
                   .AddSingleton<PhishingGatewayService>()
                   .AddHostedService(s => s.GetRequiredService<PhishingGatewayService>())
                   .AddScoped<PhishingDetectionService>()
                   .AddCondition<RequireNSFWCondition>()
                   .AddCondition<RequireTeamOrOwnerCondition>()
                   .AddSingleton<MemberScannerService>()
                   .AddScoped<IPrefixCacheService, PrefixCacheService>()
                   .AddSingleton<IInfractionService, InfractionService>()
                   .AddHostedService(s => (s.GetRequiredService<IInfractionService>() as InfractionService)!)
                   .AddSingleton<InviteDetectionService>()
                   .AddSingleton<ExemptionEvaluationService>()
                   .AddSingleton<IChannelLoggingService, ChannelLoggingService>()
                   .AddSingleton<MemberLoggerService>()
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