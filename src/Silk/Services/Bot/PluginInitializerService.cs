using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Remora.Plugins;
using Remora.Plugins.Abstractions;
using Remora.Plugins.Errors;
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
        if ((await LoadPluginsAsync(stoppingToken)).IsSuccess)
        {
            _logger.LogInformation("Plugins loaded successfully.");
        }
        else
        {
            _logger.LogError("Failed to load plugins.");
            _lifetime.StopApplication();
        }
    }

    private async Task<Result> LoadPluginsAsync(CancellationToken ct)
    {
        try
        {
            _logger.LogDebug("Migrating plugins...");

            var migrationResult = await _plugins.MigrateAsync(_services, ct);

            if (migrationResult.IsSuccess)
            {
                _logger.LogInformation("Migrated plugins successfully.");
            }
            else
            {
                _logger.LogError("Failed to migrate plugins.");
                return migrationResult;
            }

            _logger.LogInformation("Initializing Plugins...");
            
            var pluginInitializationResult = await _plugins.InitializeAsync(_services, ct);

            if (pluginInitializationResult.IsSuccess)
            {
                _logger.LogInformation("Initialized Plugins successfully.");
            }
            else
            {
                _logger.LogError("Failed to initialize plugins.");
                return pluginInitializationResult;
            }
            
            _logger.LogInformation("Plugins loaded!");

            return Result.FromSuccess();
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "A critical error occurred while loading plugins.");
            return Result.FromError(new ExceptionError(e));
        }
    }

}