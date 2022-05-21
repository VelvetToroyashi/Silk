using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Commands.Services;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Silk.Responders;

public class SlashCommandRegisterer : IResponder<IReady>, IResponder<IGuildCreate>
{
    private readonly IConfiguration           _config;
    private readonly SlashService             _slash;
    private readonly IHostApplicationLifetime _lifetime;
    
    public SlashCommandRegisterer(IConfiguration config, SlashService slash, IHostApplicationLifetime lifetime)
    {
        _config   = config;
        _slash    = slash;
        _lifetime = lifetime;
    }
    
    public Task<Result> RespondAsync(IReady gatewayEvent, CancellationToken ct = default)
    {
        var slashResult = _slash.SupportsSlashCommands("silk_slash_tree");

        if (!slashResult.IsSuccess)
        {
            _lifetime.StopApplication();
        }
        
        return Task.FromResult(slashResult);
    }

    public async Task<Result> RespondAsync(IGuildCreate gatewayEvent, CancellationToken ct = default)
    {
        if (gatewayEvent.IsUnavailable.IsDefined(out var unavailable) && unavailable)
            return Result.FromSuccess();
        
        var config = _config.GetSilkConfigurationOptionsFromSection();

        if (config.SlashCommandsGuildId != null)
        {
            if (config.SlashCommandsGuildId != gatewayEvent.ID.Value)
                return Result.FromSuccess();
        }
        
        return await _slash.UpdateSlashCommandsAsync(gatewayEvent.ID, "silk_slash_tree", ct);
    }
}