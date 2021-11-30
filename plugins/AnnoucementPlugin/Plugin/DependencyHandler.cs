using AnnoucementPlugin.Database;
using AnnoucementPlugin.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using YumeChan.PluginBase;
using YumeChan.PluginBase.Tools.Data;

namespace AnnoucementPlugin
{
	public sealed class DependencyHandler : DependencyInjectionHandler
	{
		public override IServiceCollection ConfigureServices(IServiceCollection services)
		{
			return services
				.AddMediatR(typeof(DependencyHandler))
				.AddSingleton<AnnouncementService>()
				.AddDbContext<AnnouncementContext>((p, b) =>
				{
					p.GetService<IDatabaseProvider<AnnouncementPlugin>>()!
						.GetPostgresContextOptionsBuilder()(b);
				})
				.AddSingleton<IMessageDispatcher, MessageDispatcher>();
		}
	}
}