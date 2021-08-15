using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using YumeChan.PluginBase;

namespace TestPlugin
{
	public class PluginManifest : Plugin
	{
		private readonly ILogger<PluginManifest> _logger;
		public override string DisplayName => "Silk! Test Plugin";

		public PluginManifest(ILogger<PluginManifest> logger)
		{
			_logger = logger;
		}

		public override async Task LoadAsync()
		{
			await base.LoadAsync();
			_logger.LogInformation("Loaded successfully!");
		}
	}
}