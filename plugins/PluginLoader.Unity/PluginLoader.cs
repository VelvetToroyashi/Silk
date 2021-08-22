using System.Collections.Generic;
using Microsoft.Extensions.Logging;

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
		
		public PluginLoader(ILogger<PluginLoader> logger) => _logger = logger;

		
		
	}
}