using AnnoucementPlugin.Services;
using Microsoft.Extensions.DependencyInjection;
using YumeChan.PluginBase;

namespace AnnoucementPlugin
{
	public sealed class DependencyHandler : DependencyInjectionHandler
	{
		public override IServiceCollection ConfigureServices(IServiceCollection services)
			=> services
				.AddSingleton<AnnouncementService>()
				.AddSingleton<IMessageDispatcher, MessageDispatcher>();
	}
}