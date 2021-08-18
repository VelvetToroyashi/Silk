using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using YumeChan.PluginBase;

namespace PluginLoader.Unity
{
	public sealed record PluginManifest
	{
		public Plugin Plugin { get; set; }
		public Assembly Assembly { get; init; }
		public FileInfo PluginInfo { get; init; }
		public AssemblyLoadContext LoadContext { get; init; }


		public static implicit operator Plugin(PluginManifest manifest) => manifest.Plugin;
	}
}