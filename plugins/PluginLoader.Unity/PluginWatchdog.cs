using System.IO;
using Microsoft.Extensions.Logging;

namespace PluginLoader.Unity
{
	public sealed class PluginWatchdog
	{
		private readonly FileSystemWatcher _fileWatcher = new("./plugins", "*Plugin.dll");
		
		private readonly ILogger<PluginWatchdog> _logger;
		private readonly PluginLoader _loader;
		
		public PluginWatchdog(ILogger<PluginWatchdog> logger, PluginLoader loader)
		{
			_logger = logger;
			_loader = loader;

			_fileWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
			_fileWatcher.EnableRaisingEvents = true;
			
			_fileWatcher.Created += LoadPlugin;
			_fileWatcher.Changed += ReloadPlugin;
			_fileWatcher.Deleted += UnloadPlugin;
		}

		private async void UnloadPlugin(object sender, FileSystemEventArgs e)
			=> await _loader.UnloadPlugin(new FileInfo(e.FullPath));
		private async void ReloadPlugin(object sender, FileSystemEventArgs e)
			=> await _loader.LoadNewPluginAsync(new FileInfo(e.FullPath));
		

		private async void LoadPlugin(object sender, FileSystemEventArgs e)
			=> await _loader.LoadNewPluginAsync(new FileInfo(e.FullPath));
		


	}
}