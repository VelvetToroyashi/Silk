using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PluginLoader.Unity;
using Silk.Core.EventHandlers.Messages;
using Silk.Core.SlashCommands;
using Silk.Core.SlashCommands.Commands;
using Silk.Core.Utilities;
using Silk.Core.Utilities.Bot;
using Silk.Core.Utilities.HelpFormatter;
using Silk.Extensions.DSharpPlus;
using Silk.Shared.Constants;

namespace Silk.Core
{
	public sealed class Main : IHostedService
	{
		private readonly ILogger<Main> _logger;
		private readonly IPluginLoaderService _plugins;
		private readonly DiscordClient _client;
		private readonly BotExceptionHandler _handler;
		private readonly CommandHandler _commandHandler;
		private readonly SlashCommandExceptionHandler _slashExceptionHandler;

		public Main(
			DiscordClient client,
			ILogger<Main> logger,
			EventHelper e,
			BotExceptionHandler handler,
			CommandHandler commandHandler,
			SlashCommandExceptionHandler slashExceptionHandler, 
			IPluginLoaderService plugins, PluginWatchdog wd) // About the EventHelper: Consuming it in the ctor causes it to be constructed,
		{
			// And that's all it needs, since it subs to events in it's ctor.
			_logger = logger; // Not ideal, but I'll figure out a better way. Eventually. //
			_handler = handler;
			_commandHandler = commandHandler;
			_slashExceptionHandler = slashExceptionHandler;
			_plugins = plugins;
			_client = client;
			_ = e;
			_ = wd;
		}


		public async Task StartAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation(EventIds.Core, "Starting Service");

			await InitializeClientExtensions();
			_logger.LogInformation(EventIds.Core, "Initialized Client");
			
			await InitializeCommandsNextAsync();
			await InitializeSlashCommandsAsync();

			await _handler.SubscribeToEventsAsync();
			
			_logger.LogDebug(EventIds.Core, "Connecting to Discord Gateway");
			await _client.ConnectAsync();
			_logger.LogInformation(EventIds.Core, "Connected to Discord Gateway as {User}", _client.CurrentUser.ToDiscordName());
			
			await _plugins.LoadPluginsAsync();
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation(EventIds.Core, "Stopping Service");
			_logger.LogDebug(EventIds.Core, "Disconnecting from Discord Gateway");
			await _client.DisconnectAsync();
			_logger.LogInformation(EventIds.Core, "Disconnected from Discord Gateway");
		}

		private async Task InitializeClientExtensions()
		{
			_logger.LogDebug(EventIds.Core, "Initializing Client");

			_client.UseCommandsNext(DiscordConfigurations.CommandsNext);
			_client.UseInteractivity(DiscordConfigurations.Interactivity);
			_client.UseVoiceNext(DiscordConfigurations.VoiceNext);
		}

		private Task InitializeSlashCommandsAsync()
		{
			_logger.LogInformation(EventIds.Core, "Initializing Slash-Commands");
			SlashCommandsExtension? sc = _client.UseSlashCommands(DiscordConfigurations.SlashCommands);
			sc.SlashCommandErrored += _slashExceptionHandler.Handle;
			sc.RegisterCommands<RemindCommands>(721518523704410202);
			sc.RegisterCommands<TagCommands>(721518523704410202);
			sc.RegisterCommands<AvatarCommands>(721518523704410202);

			return Task.CompletedTask;
		}

		private async Task InitializeCommandsNextAsync()
		{
			_logger.LogInformation(EventIds.Core, "Initializing Command Framework");

			var t = Stopwatch.StartNew();
			var asm = Assembly.GetEntryAssembly();
			var cnext = _client.GetCommandsNext();

			cnext.RegisterCommands(asm);
			cnext.SetHelpFormatter<HelpFormatter>();
			cnext.RegisterConverter(new MemberConverter());
			cnext.RegisterConverter(new InfractionTypeConverter());
			cnext.CommandExecuted += _commandHandler.AddCommandInvocation;

			t.Stop();
			int registeredCommands = cnext.RegisteredCommands.Count;

			_logger.LogDebug(EventIds.Core, "Registered {Commands} core commands in {Time} ms", registeredCommands, t.ElapsedMilliseconds);

			//await _plugins.RegisterPluginCommandsAsync();
			_logger.LogInformation(EventIds.Core, "Initialized Command Framework");
		}
	}
}