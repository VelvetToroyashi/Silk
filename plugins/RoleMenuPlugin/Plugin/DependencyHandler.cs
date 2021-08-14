using MediatR;
using Microsoft.Extensions.DependencyInjection;
using YumeChan.PluginBase;

namespace RoleMenuPlugin
{
	public sealed class DependencyHandler : InjectionRegistry
	{
		public override IServiceCollection ConfigureServices(IServiceCollection services)
			=> services
				.AddSingleton<RoleMenuRoleService>()
				.AddMediatR(typeof(DependencyHandler));
	}
}