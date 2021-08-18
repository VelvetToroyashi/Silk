using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Unity;

namespace PluginLoader.Unity
{
	public sealed class PluginWatchdog
	{
		private readonly FileSystemWatcher _fileWatcher = new("./plugins", "*Plugin.dll");
		
		private readonly IPluginLoaderService _loaderService;
		private readonly ILogger<PluginWatchdog> _logger;
		private readonly IUnityContainer _container;
		private readonly PluginLoader _loader;
		
		public PluginWatchdog(ILogger<PluginWatchdog> logger, PluginLoader loader, IUnityContainer container, IPluginLoaderService loaderService)
		{
			_logger = logger;
			_loader = loader;
			_container = container;
			_loaderService = loaderService;

			_fileWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
			_fileWatcher.EnableRaisingEvents = true;
			
			_fileWatcher.Created += LoadPlugin;
			_fileWatcher.Changed += ReloadPlugin;
			_fileWatcher.Deleted += UnloadPlugin;
			_logger.LogInformation("Watchdog active.");
		}

		private async void UnloadPlugin(object sender, FileSystemEventArgs e)
		{
			_logger.LogDebug("{File} has been removed from the plugins directory. Unloading...", e.Name);

			var plugin = _loader.Plugins.SingleOrDefault(p => p.Assembly.Location == e.FullPath);
			await _loader.UnloadPlugin(new FileInfo(e.FullPath));
			
			if (plugin is not null)
				await _loaderService.UnloadPluginCommandsAsync(new[] { plugin });
		}
		private async void ReloadPlugin(object sender, FileSystemEventArgs e)
		{
			_logger.LogDebug("{File} has been updated. Reloading plugin...", e.Name);
			await LoadPluginAsync(e);
		}
		
		private async void LoadPlugin(object sender, FileSystemEventArgs e)
		{
			_logger.LogDebug("Discovered new file: {File} attempting to load plugin", e.Name);
			await LoadPluginAsync(e);
		}
		
		private async Task LoadPluginAsync(FileSystemEventArgs e)
		{
			await _loader.LoadNewPluginManifestAsync(new FileInfo(e.FullPath));
			var plugin = _loader.Plugins.Last();
			_loader.InstantiatePluginServices(_container, new[] { plugin });
			
			_loader.AddPlugins(_container);

			if (plugin.Plugin is null)
			{
				_logger.LogWarning("Plugin loading failed. See logs for more details.");
				return;
			}
			
			_logger.LogDebug("Attempting to activate plugin {Plugin}", plugin.Plugin.AssemblyName);
			
			try
			{
				await plugin.Plugin.LoadAsync();
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Plugin {Plugin} v{Version} failed to load.", plugin.Plugin.DisplayName, plugin.Plugin.Version);
				return;
			}
			_logger.LogDebug("Succefully loaded {Plugin} v{Version}", plugin.Plugin.DisplayName, plugin.Plugin.Version);
			
		}






	}
}