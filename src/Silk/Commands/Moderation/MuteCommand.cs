using System;
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
using Silk.Utilities;

namespace Silk.Commands.Moderation;

[Category(Categories.Mod)]
public class MuteCommand : CommandGroup
{
    private readonly ICommandContext        _context;
    private readonly IDiscordRestChannelAPI _channels;
    private readonly IInfractionService     _infractions;
    
    public MuteCommand(ICommandContext context, IDiscordRestChannelAPI channels, IInfractionService infractions)
    {
        _context     = context;
        _channels    = channels;
        _infractions = infractions;
    }

    [Command("mute", "429", "423")]
    [RequireContext(ChannelContext.Guild)]
    [RequireDiscordPermission(DiscordPermission.ManageRoles)]
    [Description("Mutes a user either temporarily. Muting an already muted member will update the mute time.")]
    public async Task<Result> MuteAsync
    (
        [NonSelfActionable]
        IUser user,
        
        [Description("The amount of time to mute the user for.")]
        TimeSpan? duration,
        
        [Greedy] 
        [Description("The reason for the mute.")]
        string reason = "Not Given."
    )
    {
        var infractionResult = await _infractions.MuteAsync(_context.GetGuildID(), user.ID, _context.GetUserID(), reason, duration);
        var notified         = infractionResult.IsDefined(out var result) && result.Notified ? "(User notified via DM)" : "(Failed to DM)";
        
        return (Result)await _channels.CreateMessageAsync
        (
         _context.GetChannelID(),
         !infractionResult.IsSuccess
             ? infractionResult.Error!.Message
             : $"{Emojis.MuteEmoji} Successfully muted {user.ToDiscordTag()}! {notified}"
        );
    }
}