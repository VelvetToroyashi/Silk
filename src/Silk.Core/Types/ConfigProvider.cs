using System.IO;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using YumeChan.PluginBase.Tools;

[assembly: InternalsVisibleTo ("DynamicProxyGenAssembly2")]
namespace Silk.Core.Types
{
	public sealed class ConfigProvider<T> : IConfigProvider<T> where T : class
	{
		public T InitConfig(string filename)
		{
			ConfigFile = new($"./config/{filename}.json");
			if (!File.Exists(ConfigFile.ToString()))
				return default(T);

			var file = File.ReadAllText(ConfigFile.ToString());
			
			return JsonConvert.DeserializeObject<T>(file);
		}
		
		public T Configuration { get => _configuration ?? InitConfig(typeof(T).Assembly.GetName().Name!); set => _configuration = value; }

		private T? _configuration;
		
		public FileInfo ConfigFile { get; private set; }
	}
}