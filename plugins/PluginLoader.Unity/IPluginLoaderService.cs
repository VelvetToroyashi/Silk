using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YumeChan.PluginBase;

namespace PluginLoader.Unity
{
	public interface IPluginLoaderService
	{
		/// <summary>
		///     Attempts to load all plugins in the plugin directory asynchronously.
		/// </summary>
		/// <remarks>
		///     In the event that the exception parameter is <see cref="AggregateException"/>, the plugin threw an exception that was caught,
		///     and unloading the plugin also threw an exception. The first exception is the exception from unloading, and the second is the exception from
		///     loading.
		/// </remarks>
		/// <returns>A collection of plugins that failed to load (or unload, if they failed to load.)</returns>
		Task LoadPluginsAsync();
		/// <summary>
		///     Registers loaded plugins' applicable commands
		/// </summary>
		/// <returns></returns>
		Task RegisterPluginCommandsAsync();
		Task RegisterPluginCommandsAsync(IEnumerable<Plugin> plugins);
		Task UnloadPluginCommandsAsync(IEnumerable<PluginManifest> plugins);
	}
}