using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RoleMenuPlugin.Database;
using YumeChan.PluginBase;
using YumeChan.PluginBase.Tools.Data;

namespace RoleMenuPlugin
{
	public sealed class DependencyHandler : DependencyInjectionHandler
	{
		public override IServiceCollection ConfigureServices(IServiceCollection services)
		{
			return services
				.AddMediatR(typeof(DependencyHandler))
				.AddDbContext<RoleMenuContext>((provider, builder) =>
				{
					Action<DbContextOptionsBuilder>? applyDb = provider
						.GetService<IDatabaseProvider<RoleMenuPlugin>>()!
						.GetPostgresContextOptionsBuilder();
					applyDb(builder);
				})
				.AddSingleton<RoleMenuRoleService>();
		}
	}
}