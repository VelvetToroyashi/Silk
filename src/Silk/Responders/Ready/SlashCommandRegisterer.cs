using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Gateway.Events;
using Remora.Discord.Commands.Services;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Silk.Responders;

public class SlashCommandRegisterer : IResponder<IReady>
{

    private readonly SlashService             _slash;
    private readonly IHostApplicationLifetime _lifetime;
    
    public SlashCommandRegisterer(SlashService slash, IHostApplicationLifetime lifetime)
    {
        _slash    = slash;
        _lifetime = lifetime;
    }
    
    public Task<Result> RespondAsync(IReady gatewayEvent, CancellationToken ct = default)
    {
        var slashResult = _slash.SupportsSlashCommands("silk_slash_tree");

        if (!slashResult.IsSuccess)
        {
            _lifetime.StopApplication();
            return Task.FromResult(slashResult);
        }
        
        return _slash.UpdateSlashCommandsAsync(null, "silk_slash_tree", ct);
    }
}