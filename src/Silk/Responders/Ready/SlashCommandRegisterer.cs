using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Commands.Services;
using Remora.Discord.Gateway.Responders;
using Remora.Results;
using Silk.Shared.Configuration;

namespace Silk.Responders;

public class SlashCommandRegisterer : IResponder<IReady>
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

        if (_config.GetSilkConfigurationOptions().SlashCommandsGuildId is { } slashDebugGuild)
        {
            return _slash.UpdateSlashCommandsAsync(DiscordSnowflake.New(slashDebugGuild), "silk_slash_tree", ct);
        }
        else
        {
            return _slash.UpdateSlashCommandsAsync(null, "silk_slash_tree", ct);
        }
    }
}