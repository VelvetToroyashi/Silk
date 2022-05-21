using System.ComponentModel;
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

[Category(Categories.Mod)]
public class KickCommand : CommandGroup
{
    private readonly ICommandContext        _context;
    private readonly IInfractionService     _infractions;
    private readonly IDiscordRestChannelAPI _channels;
        
    public KickCommand
    (
        IInfractionService     infractions,
        ICommandContext        context,
        IDiscordRestChannelAPI channels
    )
    {
        _context     = context;
        _infractions = infractions;
        _channels    = channels;
    }


    [Command("kick")]
    [RequireContext(ChannelContext.Guild)]
    [Description("Boot someone from the guild!")]
    [RequireDiscordPermission(DiscordPermission.KickMembers)]
    public async Task<IResult> Kick([NonSelfActionable] IUser user, [Greedy] string reason = "Not given.")
    {
        var infractionResult = await _infractions.KickAsync(_context.GuildID.Value, user.ID, _context.User.ID, reason);
        var notified         = infractionResult.Entity.UserNotified ? "(User notified via DM)" : "(Failed to DM)";
        
        return await _channels.CreateMessageAsync
            (_context.ChannelID,
             !infractionResult.IsSuccess
                 ? infractionResult.Error.Message
                 : $"{Emojis.KickEmoji} Kicked **{user.ToDiscordTag()}**! {notified}"
            );

    }
}