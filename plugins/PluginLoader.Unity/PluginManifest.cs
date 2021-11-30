using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using YumeChan.PluginBase;

namespace PluginLoader.Unity
{
	/// <summary>
	///     A data-like class that holds information about a plugin, including the instance of the plugin itself.
	/// </summary>
	public sealed record PluginManifest
	{
		public Plugin Plugin { get; internal set; }
		public Assembly Assembly { get; init; }
		public FileInfo PluginInfo { get; init; }
		public AssemblyLoadContext LoadContext { get; init; }

		public static implicit operator Plugin(PluginManifest manifest)
		{
			return manifest.Plugin;
		}
	}
}