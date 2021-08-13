using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using YumeChan.PluginBase;

namespace TestPlugin
{
	public class TestPlugin : Plugin
	{
		public override string PluginDisplayName { get; } = "TestPlugin";
		public override bool PluginStealth { get; } = false;

		private readonly ILogger<TestPlugin> _logger;

		public override async Task LoadPlugin()
		{
			await base.LoadPlugin();
			_logger.LogInformation("Test plugin loaded!");
		}

		public TestPlugin(ILogger<TestPlugin> logger)
		{
			_logger = logger;
			logger.LogInformation("Instantiated! Pog!");
		}
	}
	
	public class TestInstantiator : InjectionRegistry
	{
		public override IServiceCollection ConfigureServices(IServiceCollection services)
		{
			return null;
		}
	}

}
