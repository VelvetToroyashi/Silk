using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Remora.Commands.Extensions;
using Remora.Commands.Tokenization;
using Remora.Discord.API.Abstractions.Gateway.Commands;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Caching.Extensions;
using Remora.Discord.Caching.Services;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Gateway;
using Remora.Discord.Hosting.Extensions;
using Remora.Extensions.Options.Immutable;
using Serilog;
using Serilog.Events;
using Serilog.Templates;
using Silk.Commands.Conditions;
using Silk.Extensions;
using Silk.Extensions.Remora;
using Silk.Shared;
using Silk.Shared.Configuration;
using Silk.Shared.Constants;
using Silk.Utilities.HelpFormatter;
using IChannel = MongoDB.Driver.Core.Bindings.IChannel;

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
            //.AddInteractivity()
           .AddResponders(asm);

        services
           .AddScoped<CommandHelpViewer>()
           .AddScoped<IHelpFormatter, HelpFormatter.HelpFormatter>();

        services.AddCondition<NonSelfActionableCondition>()
                .AddCondition<RequireNSFWCondition>();
        
        services
           .AddDiscordCommands(useDefaultCommandResponder: false)
           .AddDiscordCaching();
        
        services
            //.AddPostExecutionEvent<FailedCommandResponder>()
           .AddCommands(asm) // Register types
           .AddCommands();   // Register commands
        //.Replace(ServiceDescriptor.Scoped<CommandResponder>(s => s.GetRequiredService<SilkCommandResponder>()));
        
        services.AddParser<EmojiParser>();

        services.AddPostExecutionEvent<AfterSlashHandler>();

        services
           .Configure<DiscordGatewayClientOptions>(gw =>
            {
                gw.Intents |=
                    GatewayIntents.GuildMembers   |
                    GatewayIntents.GuildPresences |
                    GatewayIntents.Guilds         |
                    GatewayIntents.DirectMessages |
                    GatewayIntents.GuildMessages;
            })
           .Configure<CacheSettings>(cs =>
            {
                cs.SetAbsoluteExpiration<IChannel>(null)
                  .SetAbsoluteExpiration<IMessage>(null)
                  .SetAbsoluteExpiration<IUser>(TimeSpan.FromHours(12))
                  .SetAbsoluteExpiration<IGuildMember>(TimeSpan.FromMinutes(5));
            })
           .Configure<TokenizerOptions>(t => t with { RetainQuotationMarks = true });

        return services;
    }

    public static IServiceCollection AddSilkLogging(this IServiceCollection services, HostBuilderContext host)
    {
        LoggerConfiguration logger = new LoggerConfiguration()
                                    .Enrich.FromLogContext()
                                    .WriteTo.Console(new ExpressionTemplate(StringConstants.LogFormat, theme: SilkLogTheme.TemplateTheme))
                                    .WriteTo.File("./logs/silkLog.log", LogEventLevel.Verbose, StringConstants.FileLogFormat, retainedFileCountLimit: null, rollingInterval: RollingInterval.Day, flushToDiskInterval: TimeSpan.FromMinutes(1))
                                    .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                                    .MinimumLevel.Override("DSharpPlus", LogEventLevel.Warning)
                                    .MinimumLevel.Override("System.Net", LogEventLevel.Fatal);

        string? configOptions = host.Configuration["Logging"];
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