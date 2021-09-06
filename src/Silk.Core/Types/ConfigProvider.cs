using System.IO;
using Config.Net;
using YumeChan.PluginBase.Tools;

namespace Silk.Core.Types
{
	public sealed class ConfigProvider<T> : IConfigProvider<T> where T : class
	{
		public T InitConfig(string filename)
		{
			ConfigFile = new FileInfo($"./config/{filename}.json");

			return Configuration = new ConfigurationBuilder<T>().UseJsonFile(ConfigFile.ToString()).Build();
		}
		
		public T Configuration { get => _configuration ?? InitConfig(typeof(T).Assembly.FullName!); set => _configuration = value; }

		private T? _configuration;
		
		public FileInfo ConfigFile { get; private set; }
	}
}