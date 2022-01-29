using JetBrains.Annotations;
using OneOf;
using Remora.Commands.Trees;
using Remora.Commands.Trees.Nodes;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Extensions;
using Remora.Rest.Core;
using Remora.Results;

namespace Silk.Remora.SlashCommands;

[PublicAPI]
public class SlashCommandService
{
    private readonly CommandTree                _commandTree;
    private readonly IDiscordRestOAuth2API      _oauth2API;
    private readonly IDiscordRestApplicationAPI _applicationAPI;
    
    public IReadOnlyDictionary<SlashCommandIdentifier, OneOf<CommandNode, IReadOnlyDictionary<string, CommandNode>>> RegisteredCommands { get; private set; }

    public SlashCommandService(CommandTree commandTree, IDiscordRestOAuth2API oauth2API, IDiscordRestApplicationAPI applicationAPI)
    {
        _commandTree = commandTree;
        _oauth2API = oauth2API;
        _applicationAPI = applicationAPI;

        RegisteredCommands = new Dictionary<SlashCommandIdentifier, OneOf<CommandNode, IReadOnlyDictionary<string, CommandNode>>>();
    }

    public async Task<Result> UpdateSlashCommandsAsync
    (
        Snowflake?        guildID = null,
        CancellationToken ct      = default
    )
    {
        var appResult = await _oauth2API.GetCurrentBotApplicationInformationAsync(ct);

        if (!appResult.IsDefined(out var app))
            return Result.FromError(appResult);
        
        var createCommands = _commandTree.CreateApplicationCommands();
        
        if (!createCommands.IsDefined(out var commands))
            return Result.FromError(createCommands);
        
        var updateResult = await
            (
                guildID is null
                    ? _applicationAPI.BulkOverwriteGlobalApplicationCommandsAsync(app.ID, commands, ct)
                    : _applicationAPI.BulkOverwriteGuildApplicationCommandsAsync(app.ID, guildID.Value, commands, ct)
            );

        if (!updateResult.IsDefined(out var updatedCommands))
            return Result.FromError(updateResult);
        
        var mergedCommands = new Dictionary<SlashCommandIdentifier, OneOf<CommandNode, IReadOnlyDictionary<string, CommandNode>>>(RegisteredCommands);

        var updatedMappings = _commandTree.MapApplicationCommands(updatedCommands);
        
        foreach (var (key, value) in updatedMappings)
        {
            mergedCommands[key] = value;
        }
        
        RegisteredCommands = mergedCommands;
        
        return Result.FromSuccess();
    }
    
    
}