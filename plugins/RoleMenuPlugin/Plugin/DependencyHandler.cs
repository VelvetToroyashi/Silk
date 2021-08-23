using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RoleMenuPlugin.Database;
using YumeChan.PluginBase;
using YumeChan.PluginBase.Tools.Data;

namespace RoleMenuPlugin
{
	public sealed class DependencyHandler : DependencyInjectionHandler
	{
		public override IServiceCollection ConfigureServices(IServiceCollection services)
			=> services
				.AddMediatR(typeof(DependencyHandler))
				.AddDbContext<RolemenuContext>((provider, builder) =>
				{
					var applyDb = provider
						.GetService<IDatabaseProvider<RoleMenuPlugin>>()!
						.GetPostgresContextOptionsBuilder();
					applyDb(builder);
				})
				.AddSingleton<RoleMenuRoleService>();
	}
}