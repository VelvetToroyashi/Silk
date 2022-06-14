using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
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

        Console.WriteLine("Starting Silk...");
        
        
        IHostBuilder? hostBuilder = Host
                                   .CreateDefaultBuilder()
                                   .UseConsoleLifetime();
        
        hostBuilder.ConfigureAppConfiguration(configuration =>
        {
            configuration.SetBasePath(Directory.GetCurrentDirectory());
            configuration.AddJsonFile("appSettings.json", true, false);
            configuration.AddUserSecrets("VelvetThePanda-Silk", false);
        });

        ConfigureServices(hostBuilder).AddPlugins();

        hostBuilder.ConfigureServices(AddRedisAndAcquireShard);
        
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
        IHostBuilder? builder = Host
           .CreateDefaultBuilder(args);

        builder.ConfigureServices((context, container) =>
        {
            SilkConfigurationOptions? silkConfig = context.Configuration.GetSilkConfigurationOptions();

            AddDatabases(container, silkConfig.Persistence);
        });

        return builder;
    }

    private static void AddRedisAndAcquireShard(HostBuilderContext context, IServiceCollection services)
    {
        Log.ForContext<Program>().Information("Attempting to acquire shard ID from Redis...");
        var config      = context.Configuration;
        
        var silkConfig  = config.GetSilkConfigurationOptions();
        var redisConfig = silkConfig.Redis;

        var connectionConfig = new ConfigurationOptions()
        {
            EndPoints       = { { redisConfig.Host, redisConfig.Port } },
            Password        = redisConfig.Password,
            DefaultDatabase = redisConfig.Database
        };
        
        var redis = ConnectionMultiplexer.Connect(connectionConfig);
        
        var db    = redis.GetDatabase();
        var taken = false;

        var takenShard = 0;
        
        while (true)
        {
            for (int i = 0; i < silkConfig.Discord.Shards; i++)
            {
                var key = $"shard:{i}";

                if (db.KeyExists(key))
                    continue;

                db.StringSet(key, "", TimeSpan.FromSeconds(7));
                Metrics.DefaultRegistry.SetStaticLabels(new() { {"shard", i.ToString()} });
                
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
        
        services.Configure<DiscordGatewayClientOptions>(gw => gw.ShardIdentification = new ShardIdentification(takenShard, silkConfig.Discord.Shards));
    }
    
    private static async Task<Result<int>> EnsureDatabaseCreatedAndApplyMigrations(IHost builtBuilder)
    {
        try
        {
            using var serviceScope = builtBuilder.Services.CreateScope();

            await using GuildContext dbContext = serviceScope
                                                .ServiceProvider
                                                .GetRequiredService<GuildContext>();

            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();

            if (pendingMigrations.Any())
                await dbContext.Database.MigrateAsync();

            return Result<int>.FromSuccess(pendingMigrations.Count());
        }
        catch (Exception e)
        {
            return Result<int>.FromError(new ExceptionError(e));
        }
    }

    private static IHostBuilder ConfigureServices(IHostBuilder builder)
    {
        builder
           .AddPlugins()
           .AddRemoraHosting()
           .ConfigureLogging(l => l.ClearProviders().AddSerilog())
           .ConfigureServices((context, services) =>
            {
                // There's a more elegant way to do this, but I'm lazy and this works.
                var silkConfig = context.Configuration.GetSilkConfigurationOptions();

                AddDatabases(services, silkConfig.Persistence);
                
                // A little note on Sentry; it's important to initialize logging FIRST
                // And then sentry, because we set the settings for sentry later. 
                // If we configure logging after, it'll override the settings with defaults.

                services
                   .AddSilkConfigurationOptions(context.Configuration)
                   .AddRemoraServices()
                   .AddSingleton<ShardHelper>()
                   .AddHostedService<ShardStatService>()
                   .AddSilkLogging(context.Configuration)
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
                   .AddSingleton<GuildCacherService>()
                   .AddSingleton<GuildGreetingService>()
                   .AddSingleton<IClock>(SystemClock.Instance)
                   .AddSingleton<IDateTimeZoneSource>(TzdbDateTimeZoneSource.Default)
                   .AddSingleton<IDateTimeZoneProvider, DateTimeZoneCache>()
                   .AddTransient<TimeHelper>()
                   .AddHostedService(s => s.GetRequiredService<GuildGreetingService>())
                   .AddSingleton<FlagOverlayService>()
                   .AddSingleton<MessageLoggerService>()
                   .AddMediatR(typeof(Program))
                   .AddMediatR(typeof(GuildContext))
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

    private static void AddDatabases(IServiceCollection services, SilkPersistenceOptions persistenceOptions)
    {
        void Builder(DbContextOptionsBuilder b)
        {
            b.UseNpgsql(persistenceOptions.GetConnectionString());
            #if DEBUG
            b.EnableSensitiveDataLogging();
            b.EnableDetailedErrors();
            #endif // EFCore will complain about enabling sensitive data if you're not in a debug build. //
        }

        services.AddDbContext<GuildContext>(Builder, ServiceLifetime.Transient);
        services.AddDbContextFactory<GuildContext>(Builder, ServiceLifetime.Transient);
    }
}