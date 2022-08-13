using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Remora.Commands.Extensions;
using Remora.Commands.Tokenization;
using Remora.Discord.API.Abstractions.Gateway.Commands;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Caching.Services;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Services;
using Remora.Discord.Gateway;
using Remora.Discord.Gateway.Extensions;
using Remora.Discord.Gateway.Services;
using Remora.Discord.Gateway.Transport;
using Remora.Discord.Interactivity.Extensions;
using Remora.Discord.Pagination;
using Remora.Discord.Pagination.Extensions;
using Remora.Discord.Rest;
using Remora.Discord.Rest.Extensions;
using Remora.Extensions.Options.Immutable;
using Remora.Plugins.Services;
using Remora.Rest.Core;
using Remora.Results;
using Serilog;
using Serilog.Events;
using Serilog.Templates;
using Silk.Commands.Conditions;
using Silk.Data;
using Silk.Data.EntityMapping;
using Silk.Extensions.Remora;
using Silk.Infrastructure;
using Silk.Interactivity;
using Silk.Services.Bot;
using Silk.Services.Bot.Help;
using Silk.Shared;
using Silk.Shared.Configuration;
using Silk.Shared.Constants;
using Silk.Utilities.HelpFormatter;
using Silk.Utilities.HttpClient;
using VTP.Remora.Commands.HelpSystem;

namespace Silk.Utilities;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRemoraServices(this IServiceCollection services)
    {
        services.AddDiscordGateway(s => s.GetService<IOptions<SilkConfigurationOptions>>()!.Value.Discord.BotToken, 
                                   b => b.AddPolicyHandler(PollyMetricsHandler.Create()));
        
        // Add REST and tack on our own policy
        services.AddSingleton<ShardAwareGateweayHelper>();
        services.AddHostedService(s => s.GetRequiredService<ShardAwareGateweayHelper>());

        var asm = Assembly.GetEntryAssembly()!;

        services
           .AddResponders(asm)
           .AddInteractivity()
           .AddInteractionGroup<ReminderModalHandler>()
           .AddInteractionGroup<JoinEmbedButtonHandler>()
           .AddInteractionGroup<MemberScanButtonHandler>()
           .AddPagination()
           .AddSilkInteractivity();
        
        services
           .AddCondition<RequireBotDiscordPermissionsCondition>()
           .AddCondition<NonSelfActionableCondition>()
           .AddCondition<RequireNSFWCondition>();
        
        services
           .AddDiscordCommands(true)
           .AddSlashCommands(asm)
           .AddHelpSystem()
           .AddScoped<ICommandPrefixMatcher, SilkPrefixMatcher>()
           .AddScoped<ITreeNameResolver, SilkTreeNameResolver>();
        
        services
            //.AddPostExecutionEvent<FailedCommandResponder>()
           .AddCommands(asm) // Register types
           .AddCommands()
           .AddPostExecutionEvent<PostCommandReactionHandler>()
           .AddPostExecutionEvent<PostCommandHandler>()
           .AddPostExecutionEvent<CommandMetricsHandler>();   // Register commands
        //.Replace(ServiceDescriptor.Scoped<CommandResponder>(s => s.GetRequiredService<SilkCommandResponder>()));
        
        services.AddParser<EmojiParser>()
                .AddParser<MessageParser>();

        services.AddPostExecutionEvent<AfterSlashHandler>();

        services
           .AddSingleton<IShardIdentification>(s => s.GetRequiredService<IOptions<DiscordGatewayClientOptions>>().Value.ShardIdentification!)
           .Configure<HelpSystemOptions>(hso => hso.CommandCategories.AddRange(Categories.Order))
           .Configure<PaginatedAppearanceOptions>(pap => pap with { HelpText = "Use the buttons to navigate and the close button to stop."})
           .Configure<DiscordGatewayClientOptions>(gw =>
            {
                gw.Intents |=
                    GatewayIntents.GuildMembers   |
                    GatewayIntents.DirectMessages; // | GatewayIntents.MessageContents;
            })
           .Configure<CacheSettings>(cs =>
            {
                cs
                   .SetDefaultAbsoluteExpiration(TimeSpan.Zero)
                   .SetDefaultSlidingExpiration(null)
                   .SetAbsoluteExpiration<IReadOnlyList<Snowflake>>(null)
                   .SetAbsoluteExpiration<IGuildMember>(TimeSpan.FromDays(1))
                   .SetAbsoluteExpiration<IReadOnlyList<IRole>>(TimeSpan.FromMinutes(30))
                   .SetAbsoluteExpiration<IChannel>(null)
                   .SetAbsoluteExpiration<IMessage>(null)
                   .SetAbsoluteExpiration<IGuild>(null)
                   .SetAbsoluteExpiration<IUser>(TimeSpan.FromDays(7));
            })
           .Configure<TokenizerOptions>(t => t with { RetainQuotationMarks = true, IgnoreEmptyValues = false });

        return services;
    }
    
    public static IHostBuilder AddPlugins(this IHostBuilder hostBuilder)
    {
        Directory.CreateDirectory("./plugins"); // In case it doesn't exist.
        return hostBuilder.ConfigureServices((_, services) =>
        {
            var pluginOptions = new PluginServiceOptions(new[] { "./plugins" }, false);
            var pluginService = new PluginService(Options.Create(pluginOptions));
        
            var tree = pluginService.LoadPluginTree();
        
            services
               .AddSingleton(tree)
               .AddSingleton(pluginService)
               .AddHostedService<PluginInitializerService>();

            var configResult = tree.ConfigureServices(services);
            
            if (!configResult.IsSuccess)
            {
                Log.Logger.Error("Plugin configuration failed: {@Error}", 
                                 ((AggregateError)configResult.Error)
                                .Errors.Select(err => err.Error!.Message + "\n"));
            }
        });
    }

    public static IServiceCollection AddSilkDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var silkConfig = configuration.GetSilkConfigurationOptions();

        void Builder(DbContextOptionsBuilder b)
        {
            var connectionString = silkConfig.Persistence.GetConnectionString();

            #if DEBUG
            connectionString += "Include Error Detail=true;";
            // EFCore will complain about enabling sensitive data if you're not in a debug build. //
            b.EnableSensitiveDataLogging();
            b.EnableDetailedErrors();
            
            #endif

            b.UseLoggerFactory(NullLoggerFactory.Instance);
            b.UseNpgsql(connectionString);
        }

        EntityMapping.ConfigureMappings();
        //services.AddDbContextFactory<GuildContext>(Builder, ServiceLifetime.Scoped);
        services.AddDbContext<GuildContext>(Builder);

        return services;
    }

    public static IServiceCollection AddSilkLogging(this IServiceCollection services, IConfiguration configuration)
    {
        var config = configuration.GetSilkConfigurationOptions();

        LoggerConfiguration logger = new LoggerConfiguration()
                                    .Enrich.FromLogContext()
                                    .WriteTo.Sentry()
                                    .WriteTo.Console(new ExpressionTemplate(StringConstants.LogFormat, theme: SilkLogTheme.TemplateTheme))
                                    .WriteTo.File("./logs/silkLog.log", LogEventLevel.Verbose, StringConstants.FileLogFormat, retainedFileCountLimit: null, rollingInterval: RollingInterval.Day, flushToDiskInterval: TimeSpan.FromMinutes(1))
                                    .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                                    .MinimumLevel.Override("Remora", LogEventLevel.Information)
                                    .MinimumLevel.Override("System.Net", LogEventLevel.Fatal);

        Log.Logger = config.LogLevel switch
        {
            "All"     => logger.MinimumLevel.Verbose().CreateLogger(),
            "Info"    => logger.MinimumLevel.Information().CreateLogger(),
            "Debug"   => logger.MinimumLevel.Debug().CreateLogger(),
            "Warning" => logger.MinimumLevel.Warning().CreateLogger(),
            "Error"   => logger.MinimumLevel.Error().CreateLogger(),
            "Panic"   => logger.MinimumLevel.Fatal().CreateLogger(),
            _         => logger.MinimumLevel.Verbose().CreateLogger()
        };

        return services.AddLogging(l => l.ClearProviders().AddSerilog());
    }
    
    public static IServiceCollection AddHelpSystem(this IServiceCollection services, string? treeName = null, bool addHelpCommand = true)
    {
        services.Configure<HelpSystemOptions>(o => o.TreeName = treeName);

        if (addHelpCommand)
        {
            services
               .AddDiscordCommands()
               .AddCommandTree(treeName)
               .WithCommandGroup<HelpCommand>()
               .Finish();
        }

        services.TryAddScoped<TreeWalker>();

        services.TryAddScoped<IHelpFormatter, DefaultHelpFormatter>();
        services.TryAddScoped<ICommandHelpService, CommandHelpService>();
        
        return services;
    }
}