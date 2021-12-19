using JetBrains.Annotations;
using Remora.Commands.Trees;
using Remora.Discord.API.Abstractions.Rest;

namespace Silk.Remora.SlashCommands;

[PublicAPI]
public class SlashCommandService
{
    private readonly CommandTree                _commandTree;
    private readonly IDiscordRestOAuth2API      _oauth2API;
    private readonly IDiscordRestApplicationAPI _applicationAPI;
    
    public SlashCommandService(CommandTree commandTree, IDiscordRestOAuth2API oauth2API, IDiscordRestApplicationAPI applicationAPI)
    {
        _commandTree = commandTree;
        _oauth2API = oauth2API;
        _applicationAPI = applicationAPI;
    }
    
}