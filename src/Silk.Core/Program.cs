using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Remora.Commands.Extensions;
using Remora.Discord.Caching.Extensions;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Hosting.Extensions;
using Serilog;
using Serilog.Events;
using Serilog.Templates;
using Silk.Core.Data;
using Silk.Core.Services.Data;
using Silk.Core.Services.Interfaces;
using Silk.Core.Utilities;
using Silk.Core.Utilities.HelpFormatter;
using Silk.Extensions;
using Silk.Extensions.Remora;
using Silk.Shared;
using Silk.Shared.Configuration;
using Silk.Shared.Constants;

namespace Silk.Core
{
	public class Program
	{
		public static async Task Main()
		{
			IHostBuilder? host = Host
				.CreateDefaultBuilder()
				.UseConsoleLifetime();

			host.ConfigureAppConfiguration(configuration =>
			{
				configuration.SetBasePath(Directory.GetCurrentDirectory());
				configuration.AddJsonFile("appSettings.json", true, false);
				configuration.AddUserSecrets("VelvetThePanda-Silk", false);
			});

			ConfigureServices(host);

			await host.Build().RunAsync();
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

					services.AddLogging(_ => AddLogging(context));

					var asm = Assembly.GetExecutingAssembly();

					services.AddSingleton<IPrefixCacheService, PrefixCacheService>();
					//services.AddScoped<SilkCommandResponder>(); // So Remora's default responder can be overridden. I'll remove this when my PR is merged. //

					services
						//.AddInteractivity()
						.AddResponders(asm);

					services
						.AddScoped<CommandHelpViewer>()
						.AddScoped<IHelpFormatter, HelpFormatter>();

					services
						//.AddPostExecutionEvent<FailedCommandResponder>()
						.AddCommands(asm) // Register types
						.AddCommands(); // Register commands
					//.Replace(ServiceDescriptor.Scoped<CommandResponder>(s => s.GetRequiredService<SilkCommandResponder>()));

					services
						.AddDiscordCommands()
						.AddDiscordCaching();

					services.AddMemoryCache();

					services.AddMediatR(typeof(Program));
					services.AddMediatR(typeof(GuildContext));
				})
				.AddDiscordService(s =>
				{
					SilkConfigurationOptions? config = s.Get<IConfiguration>()!.GetSilkConfigurationOptionsFromSection();

					return config.Discord.BotToken;
				});

			return builder;
		}

		private static void AddLogging(HostBuilderContext host)
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
				"All" => logger.MinimumLevel.Verbose().CreateLogger(),
				"Info" => logger.MinimumLevel.Information().CreateLogger(),
				"Debug" => logger.MinimumLevel.Debug().CreateLogger(),
				"Warning" => logger.MinimumLevel.Warning().CreateLogger(),
				"Error" => logger.MinimumLevel.Error().CreateLogger(),
				"Panic" => logger.MinimumLevel.Fatal().CreateLogger(),
				_ => logger.MinimumLevel.Verbose().CreateLogger()
			};

			Log.Logger.ForContext(typeof(Program)).Information("Logging Initialized!");
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
		///     An extension method to get a <see cref="SilkConfigurationOptions"/> instance from the Configuration by Section Key
		/// </summary>
		/// <param name="config">the configuration</param>
		/// <returns>an instance of the SilkConfigurationOptions class, or null if not found</returns>
		public static SilkConfigurationOptions GetSilkConfigurationOptionsFromSection(this IConfiguration config)
		{
			return config.GetSection(SilkConfigurationOptions.SectionKey).Get<SilkConfigurationOptions>();
		}
	}
}