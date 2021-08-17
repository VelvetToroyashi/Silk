using System;
using System.IO;
using System.Reflection;
using YumeChan.PluginBase;

namespace PluginLoader.Unity
{
	public record PluginManifest
	{
		public Plugin Plugin { get; init; }
		public Assembly Assembly { get; init; }
		public FileInfo PluginInfo { get; init; }
		public AppDomain AssociatedDomain { get; init; }


		public static implicit operator Plugin(PluginManifest manifest) => manifest.Plugin;
	}
}