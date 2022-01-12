using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Rest.Core;
using Remora.Results;
using RoleMenuPlugin.Database;

namespace RoleMenuPlugin;

public enum RoleMenuInputState
{
    None,
    DisplayingError,
    CreatingOnMainMenu,
    CreatingOnRoleInput,
    CreatingOnEmojiInput,
    CreatingOnDescriptionInput
}

public class RoleMenuCreatorService
{


    public record RoleMenuInputData(RoleMenuInputState State, IEnumerable<RoleMenuOptionModel> Options, RoleMenuOptionModel? CurrentSelection);

    private const string RoleMenuStateKey = "rm_create-{0}-{1}";
    
    private readonly IMemoryCache                    _cache;
    private readonly RoleMenuMenuService             _menus;
    private readonly IDiscordRestInteractionAPI      _interactions;
    private readonly ILogger<RoleMenuCreatorService> _logger;
    public RoleMenuCreatorService(IMemoryCache cache, RoleMenuMenuService menus, IDiscordRestInteractionAPI interactions, ILogger<RoleMenuCreatorService> logger)
    {
        _cache        = cache;
        _menus        = menus;
        _interactions = interactions;
        _logger  = logger;
    }

    public bool IsCreating(Snowflake userID, Snowflake guildID, [NotNullWhen(true)] out RoleMenuInputData? data)
        => (data = _cache.Get<RoleMenuInputData>(string.Format(RoleMenuStateKey, userID, guildID))) is not null;

    public async Task<IResult> HandleInputAsync(Snowflake userID, Snowflake guildID, IInteraction interaction)
    {
        if (!IsCreating(userID, guildID, out var data))
        {
            _logger.LogWarning("Attempted to handle input for {User} on {Guild} when no menu was being created", userID, guildID);
            
            return Result.FromError(new InvalidOperationError("User is not creating a role menu."));
        }

        return data.State switch
        {
            RoleMenuInputState.CreatingOnMainMenu => await HandleMainMenuInputAsync(userID, guildID, interaction, data),
            _ => Result.FromSuccess()
        };

    }
    
    private async Task<IResult> HandleMainMenuInputAsync(Snowflake userID, Snowflake guildID, IInteraction interaction, RoleMenuInputData data)
    {
        return interaction.Data.Value.CustomID.Value switch
        {
            "rm-add-interactive_role" => await InitiateRoleInputAsync(userID, guildID, interaction, data),
            //"rm-add-role"             => await HandleBulkRoleInputAsync(userID, guildID, interaction, data),
            "rm-cancel"               => await CancelAsync(userID, guildID, interaction, data),
            _                         => Result.FromSuccess()
        };
    }

    private async Task<IResult> InitiateRoleInputAsync(Snowflake userID, Snowflake guildID, IInteraction interaction, RoleMenuInputData data)
    {
        data = data with { State = RoleMenuInputState.CreatingOnRoleInput, CurrentSelection = new() };
        
        _cache.Set(string.Format(RoleMenuStateKey, userID, guildID), data);

        return await _menus.DisplayRoleInputAsync(interaction);
    }

    private async Task<IResult> CancelAsync(Snowflake userId, Snowflake guildID, IInteraction interaction, RoleMenuInputData data)
    {
        data = data with { State = RoleMenuInputState.CreatingOnMainMenu, CurrentSelection = null };
        
        _cache.Set(string.Format(RoleMenuStateKey, userId, guildID), data);
        
        return await _menus.TransitionToMainMenuAsync(interaction, data.Options.ToArray());
    }
}