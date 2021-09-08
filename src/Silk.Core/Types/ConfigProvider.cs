using System.IO;
using System.Runtime.CompilerServices;
using Config.Net;
using YumeChan.PluginBase.Tools;

[assembly: InternalsVisibleTo ("DynamicProxyGenAssembly2")]
namespace Silk.Core.Types
{
	public sealed class ConfigProvider<T> : IConfigProvider<T> where T : class
	{
		public T InitConfig(string filename)
		{
			ConfigFile = new($"./plugins/config/{filename}.json");
			if (!File.Exists(ConfigFile.ToString()))
				return default(T);

			return Configuration = new ConfigurationBuilder<T>().UseJsonFile(ConfigFile.FullName).Build();
		}
		
		public T Configuration { get => _configuration ?? InitConfig(typeof(T).Assembly.GetName().Name!); set => _configuration = value; }

		private T? _configuration;
		
		public FileInfo ConfigFile { get; private set; }
	}
}