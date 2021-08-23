using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Unity;
using Unity.Microsoft.DependencyInjection;
using YumeChan.PluginBase;

namespace PluginLoader.Unity
{ 
/*
	This class is an inspired work from YumeChan's PluginLoader, which is licensed under GPL-3. 
	A copy of the GPL-3 license has been attached below for legal compliance.


	Copyright (C) 2021  Nodsoft Systems

	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

	/// <summary>
	/// A helper class that loads and instantiates plugins from assemblies located in the plugins folder.
	/// </summary>
	public sealed class PluginLoader
	{
		private readonly ILogger<PluginLoader> _logger;
		public IReadOnlyList<PluginManifest> Plugins => _plugins;
		private readonly List<PluginManifest> _plugins = new();

		private readonly IUnityContainer _container;
		
		public PluginLoader(ILogger<PluginLoader> logger, IUnityContainer container)
		{
			_logger = logger;
			_container = container;
		}

		/// <summary>
		/// Returns an array of all potential plugin files.
		/// </summary>
		/// <param name="directory">The directory to search for plugins in.</param>
		/// <returns>An array of plugin canidate file infos.</returns>
		internal FileInfo[] DiscoverPluginFiles(string directory)
		{
			if (!Directory.Exists(directory))
				return Array.Empty<FileInfo>();

			var files = Directory.GetFiles(directory, "*Plugin.dll");

			return files.Select(f => new FileInfo(f)).ToArray();
		}

		internal PluginManifest LoadPluginFile(FileInfo file) // It's up to other things to keep track of plugin states. //
		{
			var alc = new AssemblyLoadContext(file.Name, true);
			var asmStream = File.Open(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
			var asm = alc.LoadFromStream(asmStream);
			asmStream.Close();

			var manifest = new PluginManifest
			{
				Assembly = asm,
				LoadContext = alc,
				PluginInfo = file
			};
			
			_plugins.Add(manifest);
			return manifest;
		}

		internal async Task<bool> UnloadPluginFileAsync(FileInfo info)
		{
			if (_plugins.SingleOrDefault(m => m.PluginInfo.FullName == info.FullName) is not {} manifest)
				return false;

			try { await manifest.Plugin?.UnloadAsync()!; }
			catch (Exception e)
			{
				/* TODO: Log exception here or smth */
			}
			return _plugins.Remove(manifest);
		}

		internal async Task RegisterPluginAsync(PluginManifest manifest)
		{
			if (manifest.Assembly.ExportedTypes.SingleOrDefault(t => t.IsSubclassOf(typeof(DependencyInjectionHandler))) is not { } injectorType)
			{
				_logger.LogDebug("Dependency handler was not found in assembly {Asm}; assuming plugin is context-agnostic.", manifest.Assembly.FullName);
			}
			else
			{
				var injector = (DependencyInjectionHandler)_container.Resolve(injectorType);

				var services = injector.ConfigureServices(new ServiceCollection());
				_container.AddServices(services);
			
				_logger.LogDebug("Loaded services for {Plugin}", manifest.PluginInfo.Name);
			}

			if (manifest.Assembly.ExportedTypes.SingleOrDefault(t => t.IsSubclassOf(typeof(Plugin))) is not { } pluginType)
			{
				_logger.LogWarning("Plugin assembly {Asm} contains multiple plugin types. This is not supported.", manifest.Assembly.FullName);
				_plugins.Remove(manifest);
				return;
			}

			try
			{
				var plugin = (Plugin)_container.Resolve(pluginType);
				_container.RegisterInstance(typeof(Plugin), plugin);

				manifest.Plugin = plugin;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "{Plugin} failed to load. Plugin version: {Version}. Exception:", pluginType.Name, manifest.Assembly.GetName().Version!.ToString(3));
				_plugins.Remove(manifest);
			}
		}
	}
}