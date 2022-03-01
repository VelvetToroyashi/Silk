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
using Silk.Services.Interfaces;
using Silk.Utilities.HelpFormatter;
using Silk.Extensions.Remora;
using Silk.Shared.Constants;

namespace Silk.Commands.Moderation;

[HelpCategory(Categories.Mod)]
public class UnMuteCommand : CommandGroup
{
    private readonly ICommandContext        _context;
    private readonly IDiscordRestChannelAPI _channels;
    private readonly IInfractionService     _infractions;
    public UnMuteCommand(ICommandContext context, IDiscordRestChannelAPI channels, IInfractionService infractions)
    {
        _context     = context;
        _channels    = channels;
        _infractions = infractions;
    }

    [Command("unmute")]
    [RequireContext(ChannelContext.Guild)]
    [Description("Un-mutes a muted user!")]
    [RequireDiscordPermission(DiscordPermission.ManageMessages)]
    [SuppressMessage("ReSharper", "RedundantBlankLines", Justification = "Readability")]
    public async Task<IResult> UnmuteAsync
    (
        [NonSelfActionable]
        [Description("The user to un-mute.")]
        IUser user,
        
        [Greedy]
        [Description("The reason for un-muting the user.")]
        string reason = "Not Given."
    )
    {
        var infractionResult = await _infractions.UnMuteAsync(_context.GuildID.Value, user.ID, _context.User.ID, reason);
        
        return await _channels.CreateMessageAsync
            (
             _context.ChannelID,
             !infractionResult.IsSuccess
                 ? infractionResult.Error.Message
                 : $"{Emojis.UnmuteEmoji} Successfully unmuted **{user.ToDiscordTag()}**!");
        
    }
}