using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using YumeChan.PluginBase;

namespace PluginLoader.Unity
{
	/// <summary>
	/// A service for loading plugins.
	/// </summary>
	public sealed class ShardedPluginLoaderService : IPluginLoaderService
	{
		private readonly PluginLoader _loader;
		private readonly ILogger<IPluginLoaderService> _logger;
		public ShardedPluginLoaderService(PluginLoader loader, ILogger<IPluginLoaderService> logger)
		{
			_loader = loader;
			_logger = logger;
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
					_logger.LogInformation("Loaded {Plugin} v{Version}", plugin.Plugin.DisplayName, plugin.Plugin.Version);
				}
				catch (Exception e)
				{
					_logger.LogWarning(e, "Plugin {Plugin} v{Version} failed to load.", plugin.Plugin.DisplayName, plugin.Plugin.Version);
				}
			}
		}
		
		public async Task RegisterPluginCommandsAsync()
		{
			_logger.LogTrace("Soon:tm:");
		}
		
		public async Task RegisterPluginCommandsAsync(IEnumerable<Plugin> plugins) { }
		public async Task UnloadPluginCommandsAsync(IEnumerable<PluginManifest> plugins) { }
	}
}