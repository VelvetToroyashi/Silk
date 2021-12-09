using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Remora.Commands.Extensions;
using Remora.Discord.API.Abstractions.Gateway.Commands;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Caching.Extensions;
using Remora.Discord.Caching.Services;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Gateway;
using Remora.Discord.Hosting.Extensions;
using Serilog;
using Serilog.Events;
using Serilog.Templates;
using Silk.Core.Utilities;
using Silk.Core.Utilities.HelpFormatter;
using Silk.Extensions;
using Silk.Extensions.Remora;
using Silk.Shared;
using Silk.Shared.Configuration;
using Silk.Shared.Constants;
using IChannel = MongoDB.Driver.Core.Bindings.IChannel;

namespace Silk.Core;

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
           .AddScoped<IHelpFormatter, HelpFormatter>();

        services
            //.AddPostExecutionEvent<FailedCommandResponder>()
           .AddCommands(asm) // Register types
           .AddCommands();   // Register commands
        //.Replace(ServiceDescriptor.Scoped<CommandResponder>(s => s.GetRequiredService<SilkCommandResponder>()));

        services
           .AddDiscordCommands()
           .AddDiscordCaching();

        services
           .Configure<DiscordGatewayClientOptions>(gw =>
            {
                gw.Intents |=
                    GatewayIntents.GuildMembers   |
                    GatewayIntents.GuildPresences |
                    GatewayIntents.Guilds         |
                    GatewayIntents.GuildMessages;
            })
           .Configure<CacheSettings>(cs =>
            {
                cs.SetAbsoluteExpiration<IChannel>(null)
                  .SetAbsoluteExpiration<IMessage>(null);
            });

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