using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Remora.Plugins;
using Remora.Plugins.Abstractions;
using Remora.Results;

namespace Silk.Services.Bot;

public class PluginInitializerService : BackgroundService
{
    
    private readonly PluginTree                        _plugins;
    private readonly IServiceProvider                  _services;
    private readonly IHostApplicationLifetime          _lifetime;
    private readonly ILogger<PluginInitializerService> _logger;
    
    public PluginInitializerService
    (
        PluginTree plugins, 
        IServiceProvider services,
        IHostApplicationLifetime lifetime,
        ILogger<PluginInitializerService> logger
    )
    {
        _plugins  = plugins;
        _services = services;
        _lifetime = lifetime;
        _logger   = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!(await LoadPluginsAsync(stoppingToken)).IsSuccess)
        {
            _logger.LogError("Failed to load plugins.");
            _lifetime.StopApplication();
        }
    }

    private async Task<Result> LoadPluginsAsync(CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            _logger.LogDebug("Migrating plugins...");

            var migrationResult = await _plugins.MigrateAsync(_services, ct);
            
            sw.Stop();
            
            if (migrationResult.IsSuccess)
            {
                _logger.LogInformation("Migrated {Plugins} plugin(s) in {PluginTime:N0} ms.", _plugins.Branches.Count(p => p.Plugin is IMigratablePlugin), sw.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogError("One or more plugins failed to migrate: {Error}", migrationResult.Error);
                return migrationResult;
            }
            
            _logger.LogInformation("Initializing plugins...");

            sw.Restart();
            
            var pluginInitializationResult = await _plugins.InitializeAsync(_services, ct);

            if (pluginInitializationResult.IsSuccess)
            {
                _logger.LogInformation("Initialized {PluginCount} plugin(s) in {PluginTime:N0} ms.", _plugins.Branches.Count, sw.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogError("One or more plugins failed to initialize: {Error}", pluginInitializationResult.Error);
                return pluginInitializationResult;
            }
            
            return Result.FromSuccess();
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "A critical error occurred while loading plugins.");
            return Result.FromError(new ExceptionError(e));
        }
        finally
        {
            sw.Stop();
        }
    }
}