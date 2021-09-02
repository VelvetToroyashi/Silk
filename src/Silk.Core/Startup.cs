using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PluginLoader.Unity;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Silk.Core.AutoMod;
using Silk.Core.Data;
using Silk.Core.EventHandlers;
using Silk.Core.EventHandlers.Guilds;
using Silk.Core.EventHandlers.MemberAdded;
using Silk.Core.EventHandlers.MemberRemoved;
using Silk.Core.EventHandlers.Messages;
using Silk.Core.EventHandlers.Messages.AutoMod;
using Silk.Core.Services.Bot;
using Silk.Core.Services.Data;
using Silk.Core.Services.Interfaces;
using Silk.Core.Services.Server;
using Silk.Core.SlashCommands;
using Silk.Core.Utilities;
using Silk.Core.Utilities.Bot;
using Silk.Core.Utilities.HttpClient;
using Silk.Extensions;
using Silk.Shared;
using Silk.Shared.Configuration;
using Silk.Shared.Constants;
using Unity;
using Unity.Microsoft.DependencyInjection;
using Unity.Microsoft.Logging;
using YumeChan.PluginBase.Tools.Data;

namespace Silk.Core
{
	public sealed class Startup
	{
		private static IUnityContainer _container;
		
		public static async Task Main()
		{
			// Make Generic Host here. //
			IHostBuilder builder = CreateBuilder();

			ConfigureServices(builder);


			IHost builtBuilder = builder.UseConsoleLifetime().Build();
			DiscordConfigurations.CommandsNext.Services = builtBuilder.Services; // Prevents double initialization of services. //
			DiscordConfigurations.SlashCommands.Services = builtBuilder.Services;

			ConfigureDiscordClient(builtBuilder.Services);
			await EnsureDatabaseCreatedAndApplyMigrations(builtBuilder);

			await builtBuilder.RunAsync().ConfigureAwait(false);
		}

		private static async Task EnsureDatabaseCreatedAndApplyMigrations(IHost builtBuilder)
		{
			try
			{
				using IServiceScope? serviceScope = builtBuilder.Services?.CreateScope();
				if (serviceScope is not null)
				{
					await using GuildContext? dbContext = serviceScope.ServiceProvider
						.GetRequiredService<IDbContextFactory<GuildContext>>()
						.CreateDbContext();

					IEnumerable<string>? pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();

					if (pendingMigrations.Any())
						await dbContext.Database.MigrateAsync();
				}
			}
			catch (Exception)
			{
				/* Ignored. Todo: Probably should handle? */
			}
		}

		[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "EFCore CLI tools rely on reflection.")]
		public static IHostBuilder CreateHostBuilder(string[] args)
		{
			var builder = CreateBuilder();
			builder.ConfigureServices((context, container) =>
			{
				SilkConfigurationOptions? silkConfig = context.Configuration.GetSilkConfigurationOptionsFromSection();
				AddDatabases(container, silkConfig.Persistence);
			});
			
			return builder;
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

		private static void AddLogging(HostBuilderContext host)
		{
			LoggerConfiguration? logger = new LoggerConfiguration()
				.WriteTo.Console(outputTemplate: StringConstants.LogFormat, theme: new SilkLogTheme())
				.WriteTo.File("./logs/silkLog.log", LogEventLevel.Verbose, StringConstants.LogFormat, retainedFileCountLimit: null, rollingInterval: RollingInterval.Day, flushToDiskInterval: TimeSpan.FromMinutes(1))
				.MinimumLevel.Override("Microsoft", LogEventLevel.Error)
				.MinimumLevel.Override("DSharpPlus", LogEventLevel.Warning);

			SilkConfigurationOptions? configOptions = host.Configuration.GetSilkConfigurationOptionsFromSection();
			Log.Logger = configOptions.LogLevel switch
			{
				"All" => logger.MinimumLevel.Verbose().CreateLogger(),
				"Info" => logger.MinimumLevel.Information().CreateLogger(),
				"Debug" => logger.MinimumLevel.Debug().CreateLogger(),
				"Warning" => logger.MinimumLevel.Warning().CreateLogger(),
				"Error" => logger.MinimumLevel.Error().CreateLogger(),
				"Panic" => logger.MinimumLevel.Fatal().CreateLogger(),
				_ => logger.MinimumLevel.Verbose().CreateLogger()
			};
			Log.Logger.ForContext(typeof(Startup)).Information("Logging Initialized!");
		}

		private static IHostBuilder ConfigureServices(IHostBuilder builder, bool addServices = true)
		{
			return builder
				.UseUnityServiceProvider()
				.ConfigureLogging(l => l.ClearProviders())
				.UseSerilog()
				.ConfigureContainer<IUnityContainer>((context, container) =>
				{

					_container = container;
					var services = new ServiceCollection();
					SilkConfigurationOptions? silkConfig = context.Configuration.GetSilkConfigurationOptionsFromSection();

					AddSilkConfigurationOptions(services, context.Configuration);
					AddDatabases(services, silkConfig.Persistence);

					if (!addServices) return;

					if (silkConfig.Emojis?.EmojiIds is not null)
						silkConfig.Emojis.PopulateEmojiConstants();
					
					services.AddTransient(typeof(ILogger<>), typeof(Shared.Types.Logger<>));

					//services.AddSingleton(_ => new DiscordShardedClient(DiscordConfigurations.Discord));
					
					 container.RegisterFactory<DiscordShardedClient>(con =>
					 	new DiscordShardedClient(new(DiscordConfigurations.Discord) { LoggerFactory = con.Resolve<ILoggerFactory>()}), FactoryLifetime.Singleton);

					services.AddMemoryCache(option => option.ExpirationScanFrequency = TimeSpan.FromSeconds(30));

					
					services.AddHttpClient(StringConstants.HttpClientName,
						client => client.DefaultRequestHeaders.UserAgent.ParseAdd(
							$"Silk Project by VelvetThePanda / v{StringConstants.Version}"));

					services.Replace(ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, CustomLoggingFilter>());

					services.AddSingleton<GuildEventHandler>();

					#region Services

					services.AddSingleton<ConfigService>();
					services.AddSingleton<MemberGreetingService>();

					#endregion

					#region AutoMod

					services.AddSingleton<AutoModMuteApplier>();
					services.AddSingleton<AntiInviteHelper>();

					#endregion
					
					services.AddSingleton<RoleAddedHandler>();

					services.AddSingleton<MemberRemovedHandler>();
					services.AddSingleton<RoleRemovedHandler>();
					services.AddSingleton<BotExceptionHandler>();
					services.AddSingleton<SlashCommandExceptionHandler>();
					services.AddSingleton<SerilogLoggerFactory>();
					services.AddSingleton<MessageRemovedHandler>();

					services.AddSingleton<CommandHandler>();
					services.AddSingleton<MessageAddAntiInvite>();

					services.AddSingleton<EventHelper>();

					services.AddScoped<IInputService, InputService>();
					services.AddScoped<IPrefixCacheService, PrefixCacheService>();
					services.AddSingleton<IInfractionService, InfractionService>();

					services.AddSingleton<ICacheUpdaterService, CacheUpdaterService>();

					services.AddSingleton<TagService>();

					services.AddSingleton<Main>();
					services.AddHostedService(s => s.GetRequiredService<Main>());

					services.AddSingleton<IInfractionService, InfractionService>();
					services.AddHostedService(s => s.Get<IInfractionService>() as InfractionService);

					// Couldn't figure out how to get the service since AddHostedService adds it as //
					// IHostedService. Google failed me, but https://stackoverflow.com/a/65552373 helped a lot. //
					services.AddSingleton<ReminderService>();
					services.AddHostedService(b => b.GetRequiredService<ReminderService>());

					services.AddHostedService<StatusService>();

					services.AddMediatR(typeof(Main));
					services.AddMediatR(typeof(GuildContext));

					services.AddSingleton<GuildEventHandlerService>();
					services.AddHostedService(b => b.GetRequiredService<GuildEventHandlerService>());

					//services.AddSingleton<UptimeService>();
					//services.AddHostedService(b => b.GetRequiredService<UptimeService>());
					services.RegisterShardedPluginServices();
					
					services.AddSingleton(typeof(IDatabaseProvider<>), typeof(Types.DatabaseProvider<>));

					container.AddExtension(new LoggingExtension(new SerilogLoggerFactory()));
					container.AddServices(new ServiceCollection()
						.AddLogging(l =>
					{
						l.AddSerilog();
						AddLogging(context);
					}));



					container.AddExtension(new Diagnostic());
					
					container.AddServices(services); 
				});
		}


		private static void ConfigureDiscordClient(IServiceProvider services)
		{
			DiscordConfiguration client = DiscordConfigurations.Discord;
			SilkConfigurationOptions? config = services.Get<IOptions<SilkConfigurationOptions>>()!.Value;

			client.ShardCount = config!.Discord.Shards;
			client.Token = config.Discord.BotToken;
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

	/* Todo: Move this class maybe? */
	public static class IConfigurationExtensions
	{
        /// <summary>
        ///     An extension method to get a <see cref="SilkConfigurationOptions" /> instance from the Configuration by Section Key
        /// </summary>
        /// <param name="config">the configuration</param>
        /// <returns>an instance of the SilkConfigurationOptions class, or null if not found</returns>
        public static SilkConfigurationOptions GetSilkConfigurationOptionsFromSection(this IConfiguration config)
		{
			return config.GetSection(SilkConfigurationOptions.SectionKey).Get<SilkConfigurationOptions>();
		}
	}
}