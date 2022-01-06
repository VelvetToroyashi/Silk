using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using Microsoft.Extensions.DependencyInjection;
using Remora.Plugins.Abstractions;
using Remora.Plugins.Abstractions.Attributes;
using Remora.Results;
using RoleMenuPlugin.Database;

[assembly: RemoraPlugin(typeof(RoleMenuPlugin.RoleMenuPlugin))]
namespace RoleMenuPlugin;

public sealed class RoleMenuPlugin : PluginDescriptor, IMigratablePlugin
{
    public override string Name        { get; } = "Role-Menu Plugin";
    public override string Description { get; } = "Provides interaction-based role-menu functionality.";
    
    public override Result ConfigureServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<RoleMenuRoleService>();
        return Result.FromSuccess();
    }

    public async Task<Result> MigrateAsync(IServiceProvider serviceProvider, CancellationToken ct = default) => default;
}