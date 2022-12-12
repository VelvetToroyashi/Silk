using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Results;
using Silk.Commands.Conditions;
using Silk.Extensions.Remora;
using Silk.Services.Interfaces;
using Silk.Shared.Constants;
using Silk.Utilities;
using Silk.Utilities.HelpFormatter;

namespace Silk.Commands.Moderation;

[Category(Categories.Mod)]
public class StrikeCommand : CommandGroup
{
    private readonly ICommandContext        _context;
    private readonly IDiscordRestChannelAPI _channels;
    private readonly IInfractionService     _infractions;
    
    public StrikeCommand(ICommandContext context, IDiscordRestChannelAPI channels, IInfractionService infractions)
    {
        _context     = context;
        _channels    = channels;
        _infractions = infractions;
    }

    [Command("strike", "warn", "bonk", "409")]
    [RequireContext(ChannelContext.Guild)]
    [RequireDiscordPermission(DiscordPermission.ManageMessages)]
    [SuppressMessage("ReSharper", "RedundantBlankLines", Justification = "Readability")]
    [Description("Applies a strike to a user's infraction history. Until pardoned, the strike will be taken into account when evaluating automod actions.")]
    public async Task<IResult> StrikeAsync
    (
        [NonSelfActionable]
        [Description("The user to apply the strike to.")]
        IUser user,
        
        //TODO: --escalate
        [Greedy]
        [Description("The reason for the strike.")]
        string reason = "Not Given."
    )
    {
        var infractionResult = await _infractions.StrikeAsync(_context.GetGuildID(), user.ID, _context.GetUserID(), reason);
        var notified         = infractionResult.IsDefined(out var result) && result.Notified ? "(User notified via DM)" : "(Failed to DM)";
        
        return await _channels.CreateMessageAsync
            (
             _context.GetChannelID(),
             !infractionResult.IsSuccess
                 ? infractionResult.Error.Message
                 : $"{Emojis.WarningEmoji} Warned **{user.ToDiscordTag()}**! {notified}");
    }
}
