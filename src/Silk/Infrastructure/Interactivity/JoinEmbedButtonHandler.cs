using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Interactivity;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Services.Interfaces;

namespace Silk.Interactivity;

public class JoinEmbedButtonHandler : IButtonInteractiveEntity
{
    private readonly InteractionContext         _context;
    private readonly IInfractionService         _infractions;
    private readonly IDiscordRestUserAPI        _users;
    private readonly IDiscordRestInteractionAPI _interactions;
    
    public JoinEmbedButtonHandler(InteractionContext context, IInfractionService infractions, IDiscordRestUserAPI users, IDiscordRestInteractionAPI interactions)
    {
        _context      = context;
        _infractions  = infractions;
        _users        = users;
        _interactions = interactions;
    }

    public Task<Result<bool>> IsInterestedAsync(ComponentType? componentType, string customID, CancellationToken ct = default)
        => Task.FromResult(Result<bool>.FromSuccess(customID.StartsWith("join-action-")));

    
    public async Task<Result> HandleInteractionAsync(IUser user, string customID, CancellationToken ct = default)
    {
        var actionAndUser = customID[12..].Split('-');

        var action = actionAndUser[0];
        _ = Snowflake.TryParse(actionAndUser[1], out var userID);

        var permissions = _context.Member.Value.Permissions.Value;

        if (!permissions.HasPermission(DiscordPermission.Administrator) && !permissions.HasPermission(action is "kick" ? DiscordPermission.KickMembers : DiscordPermission.BanMembers))
        {
            var permissionResult = await _interactions.CreateFollowupMessageAsync
            (
                _context.ApplicationID,
                _context.Token,
                "Sorry, but you're not allowed to do that.",
                flags: MessageFlags.Ephemeral
            );

            return (Result)permissionResult;
        }
            
        var infractionResult = action switch
        {
            "ban"  => ("banned", await _infractions.BanAsync(_context.GuildID.Value, userID.Value, user.ID, 1, "Moderater-initiated action from join.")),
            "kick" => ("kicked", await _infractions.KickAsync(_context.GuildID.Value, userID.Value, user.ID, "Moderator-initiated action from join."))
        };

        var logResult = await _interactions.CreateFollowupMessageAsync
        (
            _context.ApplicationID,
            _context.Token,
            infractionResult.Item2.IsSuccess ? $"Successfully {infractionResult.Item1} user." : infractionResult.Item2.Error.Message,
            flags: MessageFlags.Ephemeral
        );

        return (Result)logResult;
    }
}