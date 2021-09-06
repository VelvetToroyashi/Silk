using Microsoft.Extensions.DependencyInjection;
using YumeChan.PluginBase;

namespace MusicPlugin
{
	public sealed class DependencyHandler : DependencyInjectionHandler
	{
		public override IServiceCollection ConfigureServices(IServiceCollection services)
		{
			return services;
		}
	}
}