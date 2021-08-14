using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dasync.Collections;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Silk.Core.Utilities.Bot;
using Unity;
using Unity.Microsoft.DependencyInjection;
using YumeChan.PluginBase;

namespace Silk.Core.Services.Bot
{
	/// <summary>
	/// A service for loading plugins.
	/// </summary>
	public sealed class PluginLoaderService
	{
		private readonly ILogger<PluginLoaderService> _logger;
		private readonly DiscordShardedClient _client;
		private readonly PluginLoader _plugins;
		private readonly IUnityContainer _container;

		private readonly FileSystemWatcher _pluginWatchdog = new("./plugins", OperatingSystem.IsWindows() ? "*Plugin*.dll" : "*Plugin*");

		public PluginLoaderService(ILogger<PluginLoaderService> logger, PluginLoader plugins, DiscordShardedClient client, IUnityContainer container)
		{
			_logger = logger;
			_plugins = plugins;
			_client = client;
			_container = container;


			_pluginWatchdog.Created += SignalPluginAdded;
		}
		private void SignalPluginAdded(object _, FileSystemEventArgs args) => Task.Run(async () => await GetNewPluginsAsync(args.Name!));
		private async Task GetNewPluginsAsync(string pluginName)
		{
			var fInfo = new FileInfo(pluginName);
			var pluginAssembly = Assembly.LoadFile(fInfo.FullName);

			var depenencyHandlers = pluginAssembly.ExportedTypes.Where(t => t.IsSubclassOf(typeof(InjectionRegistry)));
			var pluginManifests = pluginAssembly.ExportedTypes.Where(t => t.IsSubclassOf(typeof(Plugin)));

			foreach (var handler in depenencyHandlers)
				_container.AddServices((_container.Resolve(handler) as InjectionRegistry)!.ConfigureServices(new ServiceCollection()));

			var plugins = pluginManifests.Select(p => (Plugin)_container.Resolve(p));
			var failed = await LoadPluginsInternalAsync(plugins).ToListAsync();

			if (failed.Any())
			{
				foreach (var fail in failed)
				{
					_logger.LogError(fail.exception, "A plugin failed to load: {Plugin} v{PluginVersion}, defined in {PluginAsm}", fail.plugin.PluginDisplayName, fail.plugin.PluginVersion, fail.plugin.PluginAssemblyName);
				}
			}
		}

		/// <summary>
		/// Attempts to load all plugins in the plugin directory asynchronously.
		/// </summary>
		/// <remarks>In the event that the exception parameter is <see cref="AggregateException"/>, the plugin threw an exception that was caught,
		/// and unloading the plugin also threw an exception. The first exception is the exception from unloading, and the second is the exception from loading.</remarks>
		/// <returns>A collection of plugins that failed to load (or unload, if they failed to load.)</returns>
		public async Task LoadPluginsAsync()
		{
			_plugins
				.LoadPluginFiles()
				.InstantiatePluginServices(_container)
				.AddPlugins(_container);

			var failedPlugins = await LoadPluginsInternalAsync(_plugins).ToListAsync();
			
			var pluginCount = _plugins.Count();
			var failedCount = failedPlugins.Count;
			_logger.LogInformation("Loaded {Loaded} plugins out of {Total} total plugins, with {Failed} failing to load.", pluginCount - failedCount, pluginCount, failedCount);
		}


		public async Task RegisterPluginCommands()
		{
			var cnext = await _client.GetCommandsNextAsync();
			_logger.LogInformation("Initializing plugin commands");

			var sw = Stopwatch.StartNew();
			
			var beforeCount = cnext[0].RegisteredCommands.Count;
			
			foreach (var plugin in _plugins.Where(p => p.PluginLoaded))
			{
				var pluginAssembly = plugin.GetType().Assembly;
				var before = cnext[0].RegisteredCommands.Count;
				
				foreach (var ext in cnext.Values) 
					ext.RegisterCommands(pluginAssembly);

				var after = cnext[0].RegisteredCommands.Count;
				
				if (before == after)
					_logger.LogInformation("Plugin assembly contained no commands. Skipping.");
				else 
					_logger.LogInformation("Loaded {Commands} commands from {PluginName}", after - before, plugin.PluginAssemblyName);
			}
			
			sw.Stop();

			var afterCount = cnext[0].RegisteredCommands.Count;

			if (afterCount > beforeCount)
				_logger.LogInformation("Registered {TotalCommands} total commands from {Plugins} plugins. Execution time: {Time} ms", afterCount - beforeCount, _plugins.Count(), sw.ElapsedMilliseconds);
		}

		private async IAsyncEnumerable<(Plugin plugin, Exception exception)> LoadPluginsInternalAsync(IEnumerable<Plugin> plugins)
		{
			foreach (var plugin in plugins)
			{
				Exception? returnException = null;
				try
				{
					await plugin.LoadPlugin();
					_logger.LogInformation("Loaded {Plugin} v{Version}", plugin.PluginDisplayName, plugin.PluginVersion);
				}
				catch (Exception loadException)
				{
					_logger.LogWarning(loadException, "{PluginName} failed to load due to an exception. Attempting to unload.", plugin.PluginDisplayName);
					try
					{
						await plugin.UnloadPlugin();
						returnException = loadException;
						_logger.LogDebug("Plugin gracefully unloaded.");
					}
					catch (Exception unloadException)
					{
						returnException = new AggregateException(loadException, unloadException);
						_logger.LogWarning(unloadException, "Unloading a plugin resulted in an exception, Plugin: {PluginName} | Exception:", plugin.PluginDisplayName);
					}
				}

				if (returnException is not null)
					yield return (plugin, returnException);
			}
		}
	}
}