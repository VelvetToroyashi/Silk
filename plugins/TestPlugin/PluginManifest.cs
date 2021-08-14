using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using YumeChan.PluginBase;

namespace TestPlugin
{
	public class PluginManifest : Plugin
	{
		private readonly ILogger<PluginManifest> _logger;
		public override string PluginDisplayName => "Silk! Test Plugin";
		public override bool PluginStealth => false;

		public PluginManifest(ILogger<PluginManifest> logger)
		{
			_logger = logger;
		}

		public override async Task LoadPlugin()
		{
			await base.LoadPlugin();
			_logger.LogInformation("Loaded successfully!");
		}
	}
}