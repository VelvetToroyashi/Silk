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
using DSharpPlus.CommandsNext.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Unity;
using Unity.Microsoft.DependencyInjection;
using YumeChan.PluginBase;

namespace PluginLoader.Unity
{
	/// <summary>
	/// A service for loading plugins.
	/// </summary>
	public sealed class ShardedPluginLoaderService
	{
		private readonly ILogger<ShardedPluginLoaderService> _logger;
		private readonly DiscordShardedClient _client;
		private readonly PluginLoader _plugins;
		private readonly IUnityContainer _container;

		private readonly FileSystemWatcher _pluginWatchdog = new("./plugins", OperatingSystem.IsWindows() ? "*Plugin*.dll" : "*Plugin*");

		public ShardedPluginLoaderService(ILogger<ShardedPluginLoaderService> logger, PluginLoader plugins, DiscordShardedClient client, IUnityContainer container)
		{
			_logger = logger;
			_plugins = plugins;
			_client = client;
			_container = container;


			//_pluginWatchdog.Created += SignalPluginAdded;
		}
		private void SignalPluginAdded(object _, FileSystemEventArgs args) => Task.Run(async () => await GetNewPluginsAsync(args.Name!));
		private async Task GetNewPluginsAsync(string pluginName)
		{
			var fInfo = new FileInfo(pluginName);
			var pluginAssembly = Assembly.LoadFile(fInfo.FullName);

			var depenencyHandlers = pluginAssembly.ExportedTypes.Where(t => t.IsSubclassOf(typeof(DependencyInjectionHandler)));
			var pluginManifests = pluginAssembly.ExportedTypes.Where(t => t.IsSubclassOf(typeof(Plugin)));

			foreach (var handler in depenencyHandlers)
				_container.AddServices((_container.Resolve(handler) as DependencyInjectionHandler)!.ConfigureServices(new ServiceCollection()));

			var plugins = pluginManifests.Select(p => (Plugin)_container.Resolve(p));
			var failed = await LoadPluginsInternalAsync(plugins).ToListAsync();

			if (failed.Any())
			{
				foreach (var fail in failed)
				{
					_logger.LogError(fail.exception, "A plugin failed to load: {Plugin} v{PluginVersion}, defined in {PluginAsm}", fail.plugin.DisplayName, fail.plugin.Version, fail.plugin.AssemblyName);
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
			await _plugins.LoadPluginFilesAsync();
			
			_plugins.InstantiatePluginServices(_container).AddPlugins(_container);

			var failedPlugins = await LoadPluginsInternalAsync(_plugins).ToListAsync();
			
			var pluginCount = _plugins.Count();
			var failedCount = failedPlugins.Count;
			_logger.LogInformation("Loaded {Loaded} plugins out of {Total} total plugins, with {Failed} failing to load.", pluginCount - failedCount, pluginCount, failedCount);
		}
		
		/// <summary>
		/// Registers loaded plugins' applicable commadns 
		/// </summary>
		/// <returns></returns>
		public Task RegisterPluginCommandsAsync() 
			=> RegisterPluginCommandsAsync(_plugins);
		
		public async Task RegisterPluginCommandsAsync(IEnumerable<Plugin> plugins)
		{
			var cnext = await _client.GetCommandsNextAsync();
			_logger.LogInformation("Initializing plugin commands");

			var sw = Stopwatch.StartNew();
			
			foreach (var plugin in plugins.Where(p => p.Loaded))
			{
				var pluginAssembly = plugin.GetType().Assembly;
				var before = cnext[0].RegisteredCommands.Count;

				foreach (var ext in cnext.Values)
				{
					try
					{
						ext.RegisterCommands(pluginAssembly);
						var after = before - cnext[0].RegisteredCommands.Count;

						if (after is not 0)
							_logger.LogInformation("Found and registered {After} commands in {PluginName} in {Time}", after, plugin.DisplayName, sw.ElapsedMilliseconds.ToString("N0"));
					}
					catch (DuplicateCommandException e)
					{
						_logger.LogWarning("Plugin ({Plugin}) in assembly {Assembly} failed to register commadns due to naming. Command: {Command}:", plugin.DisplayName, plugin.AssemblyName, e.CommandName);
					}
					catch (Exception e)
					{
						_logger.LogError(e, "Registering commands from a plugin failed. Plugin: {Plugin} Exception:", plugin);
					}
					finally
					{
						sw.Restart();
					}
				}
			}
		}

		private async IAsyncEnumerable<(Plugin plugin, Exception exception)> LoadPluginsInternalAsync(IEnumerable<Plugin> plugins)
		{
			foreach (var plugin in plugins)
			{
				Exception returnException = null;
				try
				{
					await plugin.LoadAsync();
					_logger.LogInformation("Loaded {Plugin} v{Version}", plugin.DisplayName, plugin.Version);
				}
				catch (Exception loadException)
				{
					_logger.LogWarning(loadException, "{PluginName} failed to load due to an exception. Attempting to unload.", plugin.DisplayName);
					try
					{
						await plugin.UnloadAsync();
						returnException = loadException;
						_logger.LogDebug("Plugin gracefully unloaded.");
					}
					catch (Exception unloadException)
					{
						returnException = new AggregateException(loadException, unloadException);
						_logger.LogWarning(unloadException, "Unloading a plugin resulted in an exception, Plugin: {PluginName} | Exception:", plugin.DisplayName);
					}
				}

				if (returnException is not null)
					yield return (plugin, returnException);
			}
		}
	}
}