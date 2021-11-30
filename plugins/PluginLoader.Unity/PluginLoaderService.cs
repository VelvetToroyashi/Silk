using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using Microsoft.Extensions.Logging;
using YumeChan.PluginBase;

namespace PluginLoader.Unity
{
	/// <summary>
	///     A service for loading plugins.
	/// </summary>
	public sealed class PluginLoaderService : IPluginLoaderService
	{
		private readonly DiscordClient _client;
		private readonly PluginLoader _loader;
		private readonly ILogger<IPluginLoaderService> _logger;
		public PluginLoaderService(PluginLoader loader, ILogger<IPluginLoaderService> logger, DiscordClient client)
		{
			_loader = loader;
			_logger = logger;
			_client = client;
		}


		public async Task LoadPluginsAsync()
		{
			FileInfo[] files = _loader.DiscoverPluginFiles("./plugins");
			var manifests = new List<PluginManifest>();

			foreach (FileInfo file in files)
				manifests.Add(_loader.LoadPluginFile(file));

			foreach (PluginManifest manifest in manifests)
				await _loader.RegisterPluginAsync(manifest);

			foreach (PluginManifest plugin in manifests)
			{
				try
				{
					await plugin.Plugin.LoadAsync();
					_logger.LogInformation(Events.Plugin, "Loaded {Plugin} v{Version}", plugin.Plugin.DisplayName, plugin.Plugin.Version);
				}
				catch (Exception e)
				{
					_logger.LogWarning(Events.Plugin, e, "Plugin {Plugin} v{Version} failed to load.", plugin.Plugin?.DisplayName ?? plugin.Assembly.FullName, plugin.Plugin?.Version ?? plugin.Assembly.GetName().Version?.ToString());
				}
			}
		}

		public async Task RegisterPluginCommandsAsync()
		{
			CommandsNextExtension cnext = _client.GetCommandsNext();

			foreach (PluginManifest plugin in _loader.Plugins.Where(p => p.Plugin is not null))
			{
				try
				{
					cnext.RegisterCommands(plugin.Assembly);
				}
				catch (DuplicateCommandException e)
				{
					_logger.LogWarning(Events.Plugin, "A plugin defined as {Plugin} attempted to register a command that already existed, defined as {Command}", plugin.Plugin.DisplayName, e.CommandName);
				}
			}

		}

		public async Task RegisterPluginCommandsAsync(IEnumerable<Plugin> plugins)
		{
			CommandsNextExtension cnext = _client.GetCommandsNext();

			foreach (Plugin plugin in plugins)
			{
				try { cnext.RegisterCommands(plugin.GetType().Assembly); }
				catch (DuplicateCommandException e)
				{
					// Load the next plugin. //
					_logger.LogWarning(Events.Plugin, "A plugin defined as {Plugin} attempted to register a command that already existed, defined as {Command}", plugin?.DisplayName, e.CommandName);
				}
			}
		}
		public async Task UnloadPluginCommandsAsync(IEnumerable<PluginManifest> plugins) { }
	}
}