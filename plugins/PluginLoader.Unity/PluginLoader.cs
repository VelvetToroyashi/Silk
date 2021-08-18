using System.Collections;
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
	public sealed class PluginLoader : IEnumerable<Plugin>
	{
		private readonly ILogger<PluginLoader> _logger;
		public IReadOnlyList<PluginManifest> Plugins => _plugins;
		private readonly List<PluginManifest> _plugins = new();
		
		public PluginLoader(ILogger<PluginLoader> logger) => _logger = logger;

		/// <summary>
		/// Loads plugin manifests from Disk. Plugins must be placed in the plugins folder relative to the core binary.
		/// </summary>
		internal async Task<IEnumerable<Plugin>> LoadPluginFilesAsync()
		{
			Directory.CreateDirectory("./plugins");
			var pluginFiles = Directory.GetFiles("./plugins", "*Plugin.dll");

			foreach (var plugin in pluginFiles)
			{
				var fileInfo = new FileInfo(plugin);
				var loadContext = new AssemblyLoadContext(fileInfo.Name, true);
				var asm = loadContext.LoadFromAssemblyPath(fileInfo.FullName);
				
				var manifest = new PluginManifest()
				{
					Assembly = asm,
					PluginInfo = fileInfo,
					LoadContext = loadContext
				};

				_plugins.Add(manifest);
			}
			
			return this;
		}

		/// <summary>
		/// Instantiates services for the plugin assemblies. This should be called AFTER calling <see cref="LoadPluginFilesAsync"/>.
		/// </summary>
		/// <param name="container">The service container to add services to.</param>
		/// <param name="manifests">Optional: Specify the plugin manifests to load</param>
		internal PluginLoader InstantiatePluginServices(IUnityContainer container, IEnumerable<PluginManifest> manifests = null)
		{
			foreach (var plugin in manifests ?? _plugins)
				foreach (var t in plugin.Assembly.ExportedTypes.Where(t => t.IsSubclassOf(typeof(DependencyInjectionHandler))))
				{
					var services = (container.Resolve(t) as DependencyInjectionHandler)!.ConfigureServices(new ServiceCollection());
					
					if (services is not null)
						container.AddServices(services);
				}

			return this;
		}

		/// <summary>
		/// Instantiates plugins, but does not start them.
		/// </summary>
		/// <param name="container">The service container to add plugins to.</param>
		internal PluginLoader AddPlugins(IUnityContainer container)
		{
			foreach (var asm in _plugins)
				foreach (var t in asm.Assembly.ExportedTypes.Where(t => t.IsSubclassOf(typeof(Plugin))))
				{
					try
					{
						var plugin = container.Resolve(t) as Plugin;
						container.RegisterInstance(typeof(Plugin), plugin);
						asm.Plugin = plugin;
					}
					catch (ResolutionFailedException resex)
					{
						_logger.LogError("{Plugin} v{Version} defined as {Assembly} failed to register its services. {Type} was not provided in the container", 
							t.Name, t.Assembly.GetName().Version!.ToString(3), t.Assembly.Location, resex.TypeRequested);
					}
				}
				
			return this;
		}
		
		public IEnumerator<Plugin> GetEnumerator() => _plugins.Select(p => p.Plugin).GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}