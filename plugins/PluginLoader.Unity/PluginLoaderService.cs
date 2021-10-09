using System;
using System.Collections.Generic;
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
	/// A service for loading plugins.
	/// </summary>
	public sealed class PluginLoaderService : IPluginLoaderService
	{
		private readonly PluginLoader _loader;
		private readonly DiscordClient _client;
		private readonly ILogger<IPluginLoaderService> _logger;
		public PluginLoaderService(PluginLoader loader, ILogger<IPluginLoaderService> logger, DiscordClient client)
		{
			_loader = loader;
			_logger = logger;
			_client = client;
		}


		public async Task LoadPluginsAsync()
		{
			var files = _loader.DiscoverPluginFiles("./plugins");
			var manifests = new List<PluginManifest>();
			
			foreach (var file in files)
				manifests.Add(_loader.LoadPluginFile(file));

			foreach (var manifest in manifests)
				await _loader.RegisterPluginAsync(manifest);
			
			foreach (var plugin in manifests)
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
			var cnext = _client.GetCommandsNext();

			foreach (var plugin in _loader.Plugins.Where(p => p.Plugin is not null))
			{
				try
				{
					cnext.RegisterCommands(plugin.Assembly);
				}
				catch (DuplicateCommandException e)
				{
					_logger.LogWarning(Events.Plugin, "A plugin defined as {Plugin} attempted to register a command that already existed, defined as {Command}", plugin.Plugin.DisplayName, e.CommandName);
					break; // Next plugin. //
				}
			}
			
		}

		public async Task RegisterPluginCommandsAsync(IEnumerable<Plugin> plugins)
		{
			var cnext = _client.GetCommandsNext();
			
			foreach (var plugin in plugins)
			{
				try { cnext.RegisterCommands(plugin.GetType().Assembly); }
				catch (DuplicateCommandException e)
				{
					_logger.LogWarning(Events.Plugin, "A plugin defined as {Plugin} attempted to register a command that already existed, defined as {Command}", plugin?.DisplayName, e.CommandName);
					break; // Load the next plugin. //
				}
			}
		}
		public async Task UnloadPluginCommandsAsync(IEnumerable<PluginManifest> plugins) { }
	}
}