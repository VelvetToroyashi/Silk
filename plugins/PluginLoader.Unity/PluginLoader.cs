using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
		public AppDomain PluginDomain { get; } = AppDomain.CreateDomain("Plugins");

		// Plugin instances are held in PluginLoaderService.cs //
		private readonly List<Assembly> _pluginAssemblies = new();
		private readonly List<FileInfo> _pluginFiles = new();
		private readonly ILogger<PluginLoader> _logger;
		public IReadOnlyList<Plugin> Plugins => _plugins;
		private readonly List<Plugin> _plugins = new();
		
		public PluginLoader(ILogger<PluginLoader> logger) => _logger = logger;

		/// <summary>
		/// Loads plugin manifests from Disk. Plugins must be placed in the plugins folder relative to the core binary.
		/// </summary>
		internal PluginLoader LoadPluginFiles()
		{
			Directory.CreateDirectory("./plugins");
			var pluginFiles = Directory.GetFiles("./plugins", "*Plugin.dll");
			
			_pluginFiles.AddRange(pluginFiles.Select(f => new FileInfo(f)));
			_pluginAssemblies.AddRange(_pluginFiles.Select(f => Assembly.LoadFile(f.FullName)));
			AppDomain.CreateDomain("Plugins");
			
			
			return this;
		}
		
		/// <summary>
		/// Instantiates services for the plugin assemblies. This should be called AFTER calling <see cref="LoadPluginFiles"/>.
		/// </summary>
		/// <param name="container">The service container to add services to.</param>
		internal PluginLoader InstantiatePluginServices(IUnityContainer container)
		{
			foreach (var plugin in _pluginAssemblies)
				foreach (var t in plugin.ExportedTypes.Where(t => t.IsSubclassOf(typeof(DependencyInjectionHandler))))
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
			foreach (var asm in _pluginAssemblies)
				foreach (var t in asm.ExportedTypes.Where(t => t.IsSubclassOf(typeof(Plugin))))
				{
					try
					{
						var plugin = container.Resolve(t) as Plugin;
						container.RegisterInstance(typeof(Plugin), plugin);

						_plugins.Add(plugin!);
					}
					catch (ResolutionFailedException resex)
					{
						_logger.LogError("{Plugin} v{Version} defined as {Assembly} failed to register its services. {Type} was not provided in the container", 
							t.Name, t.Assembly.GetName().Version!.ToString(3), t.Assembly.Location, resex.TypeRequested);
					}
				}
				
			return this;
		}
		
		public IEnumerator<Plugin> GetEnumerator() => _plugins.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}