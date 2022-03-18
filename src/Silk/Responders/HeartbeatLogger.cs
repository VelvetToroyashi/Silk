using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Gateway.Bidirectional;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Silk.Responders;

public class HeartbeatLogger : BackgroundService, IResponder<IHeartbeat>, IResponder<IHeartbeatAcknowledge>
{
    private readonly ILogger<HeartbeatLogger> _logger;
    
    public static DateTimeOffset? _lastHeartbeat;
    public static DateTimeOffset? _lastHeartbeatAcknowledge;
    public HeartbeatLogger(ILogger<HeartbeatLogger> logger)
    {
        _logger = logger;
    }

    public Task<Result> RespondAsync(IHeartbeat gatewayEvent, CancellationToken ct = default)
    {
        _logger.LogInformation("Received heartbeat");
        _lastHeartbeat = DateTimeOffset.UtcNow;
        
        return Task.FromResult(Result.FromSuccess());
    }

    public Task<Result> RespondAsync(IHeartbeatAcknowledge gatewayEvent, CancellationToken ct = default)
    {
        _logger.LogInformation("Received heartbeat acknowledge in {Delta:N0}ms", DateTimeOffset.UtcNow.Subtract(_lastHeartbeat.Value).TotalMilliseconds);
        _lastHeartbeatAcknowledge = DateTimeOffset.UtcNow;
        
        return Task.FromResult(Result.FromSuccess());
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting heartbeat responder");
        
        
        while (!stoppingToken.IsCancellationRequested)
        {
            var heartbeatDelay = DateTimeOffset.UtcNow - _lastHeartbeat;
            var heartbeatAcknowledgeDelay = DateTimeOffset.UtcNow - _lastHeartbeatAcknowledge;

            if (heartbeatDelay > TimeSpan.FromSeconds(50))
            {
                _logger.LogError("Heartbeat should be within 45 seconds of the last heartbeat, but it was {Delta}", heartbeatDelay);
            }
            
            if (heartbeatAcknowledgeDelay > TimeSpan.FromSeconds(50))
            {
                _logger.LogError("Heartbeat acknowledge should be within 45 seconds of the last heartbeat acknowledge, but it was {Delta}", heartbeatAcknowledgeDelay);
            }
            
            await Task.Delay(TimeSpan.FromSeconds(47), stoppingToken);
        }
    }
}