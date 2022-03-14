using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Remora.Commands.Extensions;
using Remora.Commands.Tokenization;
using Remora.Discord.API.Abstractions.Gateway.Commands;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Caching.Extensions;
using Remora.Discord.Caching.Services;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Services;
using Remora.Discord.Gateway;
using Remora.Discord.Hosting.Extensions;
using Remora.Discord.Interactivity.Extensions;
using Remora.Discord.Pagination;
using Remora.Extensions.Options.Immutable;
using Remora.Plugins.Services;
using Remora.Results;
using Serilog;
using Serilog.Events;
using Serilog.Templates;
using Silk.Commands.Conditions;
using Silk.Extensions;
using Silk.Extensions.Remora;
using Silk.Interactivity;
using Silk.Services.Bot;
using Silk.Shared;
using Silk.Shared.Configuration;
using Silk.Shared.Constants;
using Silk.Utilities.HelpFormatter;

namespace Silk.Utilities;

public static class IServiceCollectionExtensions
{
    public static IHostBuilder AddRemoraHosting(this IHostBuilder hostBuilder)
    {
        return hostBuilder.AddDiscordService(s =>
        {
            SilkConfigurationOptions? config = s.Get<IConfiguration>()!.GetSilkConfigurationOptionsFromSection();

            return config.Discord.BotToken;
        });
    }

    public static IServiceCollection AddRemoraServices(this IServiceCollection services)
    {
        var asm = Assembly.GetEntryAssembly()!;

        services
           .AddResponders(asm)
           .AddInteractivity()
           //.AddPagination()
           .AddSilkInteractivity();

        services
           .AddScoped<CommandHelpViewer>()
           .AddScoped<IHelpFormatter, HelpFormatter.HelpFormatter>();

        services.AddCondition<NonSelfActionableCondition>()
                .AddCondition<RequireNSFWCondition>();
        
        services
           .AddDiscordCommands()
           .AddScoped<ICommandPrefixMatcher, SilkPrefixMatcher>()
           .AddDiscordCaching();
        
        services
            //.AddPostExecutionEvent<FailedCommandResponder>()
           .AddCommands(asm) // Register types
           .AddCommands()
           .AddPostExecutionEvent<PostCommandHandler>();   // Register commands
        //.Replace(ServiceDescriptor.Scoped<CommandResponder>(s => s.GetRequiredService<SilkCommandResponder>()));
        
        services.AddParser<EmojiParser>()
                .AddParser<MessageParser>();

        services.AddPostExecutionEvent<AfterSlashHandler>();

        services
           .Configure<PaginatedAppearanceOptions>(pap => pap with { HelpText = "Use the buttons to navigate and the close button to stop."})
           .Configure<DiscordGatewayClientOptions>(gw =>
            {
                gw.Intents |=
                    GatewayIntents.GuildMembers   |
                    //GatewayIntents.GuildPresences |
                    GatewayIntents.Guilds         |
                    GatewayIntents.DirectMessages |
                    GatewayIntents.GuildMessages  |
                    GatewayIntents.MessageContents;
            })
           .Configure<CacheSettings>(cs =>
            {
                
                cs
                  .SetDefaultAbsoluteExpiration(null)
                  .SetSlidingExpiration<IChannel>(null)
                  .SetSlidingExpiration<IMessage>(null)
                  .SetSlidingExpiration<IGuild>(null)
                  .SetSlidingExpiration<IUser>(TimeSpan.FromHours(12))
                  .SetSlidingExpiration<IGuildMember>(TimeSpan.FromHours(12));
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

    public static IServiceCollection AddSilkLogging(this IServiceCollection services, IConfiguration configuration)
    {
        LoggerConfiguration logger = new LoggerConfiguration()
                                    .Enrich.FromLogContext()
                                    .WriteTo.Console(new ExpressionTemplate(StringConstants.LogFormat, theme: SilkLogTheme.TemplateTheme))
                                    .WriteTo.File("./logs/silkLog.log", LogEventLevel.Verbose, StringConstants.FileLogFormat, retainedFileCountLimit: null, rollingInterval: RollingInterval.Day, flushToDiskInterval: TimeSpan.FromMinutes(1))
                                    .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                                    .MinimumLevel.Override("DSharpPlus", LogEventLevel.Warning)
                                    .MinimumLevel.Override("Remora", LogEventLevel.Error)
                                    .MinimumLevel.Override("System.Net", LogEventLevel.Fatal);

        string? configOptions = configuration["Logging"];
        Log.Logger = configOptions switch
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
}