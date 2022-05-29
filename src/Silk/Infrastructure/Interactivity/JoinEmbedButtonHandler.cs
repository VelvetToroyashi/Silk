using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
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

        var userResult = await _users.GetUserAsync(userID.Value, ct);

        if (!userResult.IsDefined())
        {
            return await _interactions.CreateInteractionResponseAsync
            (
             _context.ID,
             _context.Token,
             new InteractionResponse
                 (
                  InteractionCallbackType.ChannelMessageWithSource,
                  new
                  (
                   new InteractionMessageCallbackData
                   {
                       Content = "That user doesn't seem to exist? Oops.",
                       Flags = MessageFlags.Ephemeral
                   }
                  )
                 )
            );
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

        return logResult.IsSuccess ? Result.FromSuccess() : Result.FromError(logResult.Error);
    }
}