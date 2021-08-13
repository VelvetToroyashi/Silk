using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using YumeChan.PluginBase;

namespace Silk.Core.Utilities.Bot
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
	
	public sealed class PluginLoader
	{
		private readonly List<Assembly> _pluginAssemblies = new();
		private readonly List<FileInfo> _pluginFiles = new();

		/// <summary>
		/// Loads plugin manifests from Disk. Plugins must be placed in the plugins folder relative to the core binary.
		/// </summary>
		public PluginLoader LoadPluginFiles()
		{
			var pluginFiles = Directory.GetFiles("./plugins", $"*Plugin{(OperatingSystem.IsWindows() ? "*.dll" : "*")}");
			
			_pluginFiles.AddRange(pluginFiles.Select(f => new FileInfo(f)));
			_pluginAssemblies.AddRange(_pluginFiles.Select(f => Assembly.LoadFile(f.FullName)));

			return this;
		}
		
		/// <summary>
		/// Instantiates services for the plugin assemblies. This should be called AFTER calling <see cref="LoadPluginFiles"/>.
		/// </summary>
		/// <param name="services">The service container to add services to.</param>
		public PluginLoader InstantiatePluginServices(IServiceCollection services)
		{
			foreach (var plugin in _pluginAssemblies)
				foreach (var t in plugin.ExportedTypes.Where(t => t.IsSubclassOf(typeof(InjectionRegistry))))
					(Activator.CreateInstance(t) as InjectionRegistry)!.ConfigureServices(services);

			return this;
		}

		/// <summary>
		/// Adds the <see cref="Plugin"/>s to the container.
		/// </summary>
		/// <param name="services">The service container to add plugins to.</param>
		public PluginLoader AddPlugins(IServiceCollection services)
		{
			foreach (var asm in _pluginAssemblies)
				foreach (var t in asm.ExportedTypes.Where(t => t.IsSubclassOf(typeof(Plugin))))
					services.AddSingleton(typeof(Plugin), t);
				
			return this;
		}
	}
}