using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Remora.Commands.Extensions;
using Remora.Plugins;
using Remora.Plugins.Errors;
using Remora.Plugins.Services;
using Remora.Results;
using Serilog;
using Silk.Commands.Conditions;
using Silk.Data;
using Silk.Extensions;
using Silk.Remora.SlashCommands;
using Silk.Services.Bot;
using Silk.Services.Data;
using Silk.Services.Guild;
using Silk.Services.Interfaces;
using Silk.Shared.Configuration;
using Silk.Utilities;

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

        ConfigureServices(hostBuilder);
        
        Console.WriteLine("Configured services...");
        
        
        IHost? host = hostBuilder.Build();
        
        Console.WriteLine("Host is built. Switching to logging.");
        
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

        var pluginTree = host.Services.GetRequiredService<PluginTree>();

        Log.ForContext<Program>().Information("Attempting to migrate plugin databases");
        var migrationResult = await pluginTree.MigrateAsync(host.Services);

        if (!migrationResult.IsSuccess)
        {
            var pluginError = (AggregateError)migrationResult.Error;
            var failedPlugins = pluginError.Errors
                                           .Select(c => c.Error)
                                           .Cast<PluginMigrationFailed>();
            
            Log.ForContext<Program>().Fatal("{FailedPluginCount} plugins ({FailedPluginNames} failed to migrate.", failedPlugins.Count(),failedPlugins.Select(c => c.Descriptor.Name).Join(", "));

            return;
        }
        
        Log.ForContext<Program>().Information("Successfully migrated plugin databases.");
        
        Log.ForContext<Program>().Information("Initializing plugins...");
        
        var initalizationResult = await pluginTree.InitializeAsync(host.Services);
        
        if (!initalizationResult.IsSuccess)
        {
            var pluginError = (AggregateError)initalizationResult.Error;
            var failedPlugins = pluginError.Errors
                                           .Select(c => c.Error)
                                           .Cast<PluginInitializationFailed>();
            
            Log.ForContext<Program>().Fatal("{FailedPluginCount} plugins ({FailedPluginNames} failed to migrate.", failedPlugins.Count(),failedPlugins.Select(c => c.Descriptor.Name).Join(", "));

            return;
        }
        
        Log.ForContext<Program>().Information("Successfully initialized plugins.");
        
        Log.ForContext<Program>().Information("Startup checks OK. Starting Silk!");
        await host.RunAsync();
    }

    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "EFCore CLI tools rely on reflection.")]
    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        IHostBuilder? builder = Host
           .CreateDefaultBuilder(args);

        builder.ConfigureServices((context, container) =>
        {
            SilkConfigurationOptions? silkConfig = context.Configuration.GetSilkConfigurationOptionsFromSection();

            AddDatabases(container, silkConfig.Persistence);
        });

        return builder;
    }

    private static void AddPlugins(IServiceCollection services)
    {
        var pluginOptions = new PluginServiceOptions(new[] { "./plugins" }, false);
        var pluginService = new PluginService(Options.Create(pluginOptions));
        
        var tree = pluginService.LoadPluginTree();
        
        services.AddSingleton(pluginService)
                .AddSingleton(tree);

        var configureResult = tree.ConfigureServices(services);

        if (!configureResult.IsSuccess)
        {
            var pluginError   = (AggregateError)configureResult.Error;
            var failedPlugins = pluginError.Errors
                                           .Select(c => c.Error)
                                           .Cast<PluginConfigurationFailed>();
            
            foreach (var plugin in failedPlugins)
                Log.ForContext<Program>().Error("A plugin failed to configure: {Plugin}", plugin.Descriptor.Name);
        }
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
           .ConfigureLogging(l => l.ClearProviders().AddSerilog())
           .ConfigureServices((context, services) =>
            {
                // There's a more elegant way to do this, but I'm lazy and this works.
                SilkConfigurationOptions? silkConfig = context.Configuration.GetSilkConfigurationOptionsFromSection();

                AddSilkConfigurationOptions(services, context.Configuration);
                AddDatabases(services, silkConfig.Persistence);
                
                AddPlugins(services);

                services.AddRemoraServices();
                services.AddSilkLogging(context);

                services.AddSingleton<SlashCommandService>();

                services.AddSingleton<ReminderService>();
                services.AddHostedService(s => s.GetRequiredService<ReminderService>());

                services.AddCondition<RequireNSFWCondition>();
                services.AddCondition<RequireTeamOrOwnerCondition>();

                services.AddSingleton<IPrefixCacheService, PrefixCacheService>();
                services.AddSingleton<IInfractionService, InfractionService>();

                services.AddSingleton<InviteDectectionService>();
                services.AddSingleton<ExemptionEvaluationService>();
                
                services.AddSingleton<IChannelLoggingService, ChannelLoggingService>();
                services.AddSingleton<MemberLoggerService>();
                services.AddSingleton<GuildConfigCacheService>();
                services.AddSingleton<GuildCacherService>();

                services.AddSingleton<GuildGreetingService>();
                //services.AddScoped<SilkCommandResponder>(); // So Remora's default responder can be overridden. I'll remove this when my PR is merged. //
                
                services.AddSingleton<FlagOverlayService>();
                

                services.AddMediatR(typeof(Program));
                services.AddMediatR(typeof(GuildContext));
            })
           .AddRemoraHosting();

        return builder;
    }

    private static void AddSilkConfigurationOptions(IServiceCollection services, IConfiguration configuration)
    {
        // Add and Bind IOptions configuration for appSettings.json and UserSecrets configuration structure
        // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-5.0
        IConfigurationSection? silkConfigurationSection = configuration.GetSection(SilkConfigurationOptions.SectionKey);
        services.Configure<SilkConfigurationOptions>(silkConfigurationSection);
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
        //services.TryAdd(new ServiceDescriptor(typeof(GuildContext), p => p.GetRequiredService<IDbContextFactory<GuildContext>>().CreateDbContext(), ServiceLifetime.Transient));
    }
}

//Todo: Move this class maybe? 
public static class IConfigurationExtensions
{
    /// <summary>
    ///     An extension method to get a <see cref="SilkConfigurationOptions" /> instance from the Configuration by Section Key
    /// </summary>
    /// <param name="config">the configuration</param>
    /// <returns>an instance of the SilkConfigurationOptions class, or null if not found</returns>
    public static SilkConfigurationOptions GetSilkConfigurationOptionsFromSection(this IConfiguration config)
        => config.GetSection(SilkConfigurationOptions.SectionKey).Get<SilkConfigurationOptions>();
}