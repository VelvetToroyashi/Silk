using Microsoft.Extensions.DependencyInjection;
using MusicPlugin.Services;
using YumeChan.PluginBase;

namespace MusicPlugin.Plugin
{
	public class DependencyHandler : DependencyInjectionHandler
	{
		public override IServiceCollection ConfigureServices(IServiceCollection services)
		{
			return services.AddSingleton<MusicService>();
		}
	}
}