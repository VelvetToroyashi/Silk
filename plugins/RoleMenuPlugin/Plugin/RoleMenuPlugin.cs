using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        try
        {
            serviceCollection.AddSingleton<RoleMenuService>()
                             .AddDbContext<RoleMenuContext>((s, b) =>
                              {
                                  var config = s.GetRequiredService<IConfiguration>();
                                  var dbString = config
                                                .GetSection("Plugins")
                                                .GetSection("RoleMenu")
                                                .GetSection("Database")
                                                .Value ?? throw new KeyNotFoundException("Missing plugin config!");

                                  b.UseNpgsql(dbString);
                              });
        }
        catch (Exception e)
        {
            return Result.FromError(new ExceptionError(e));
        }
        
        
        return Result.FromSuccess();
    }

    public async Task<Result> MigrateAsync(IServiceProvider serviceProvider, CancellationToken ct = default)
    {
        var context = serviceProvider.GetRequiredService<RoleMenuContext>();

        try
        {
            await context.Database.MigrateAsync(ct);
        }
        catch (Exception e)
        {
            return Result.FromError(new ExceptionError(e));
        }
        
        return Result.FromSuccess();
    }
}