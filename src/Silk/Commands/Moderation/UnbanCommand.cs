using System.ComponentModel;
using System.Threading.Tasks;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Results;
using Silk.Extensions.Remora;
using Silk.Services.Interfaces;
using Silk.Shared.Constants;
using Silk.Utilities.HelpFormatter;

namespace Silk.Commands.Moderation;

[HelpCategory(Categories.Mod)]
public class UnbanCommand : CommandGroup
{
    private readonly ICommandContext        _context;
    private readonly IDiscordRestChannelAPI _channels;
    private readonly IInfractionService     _infractions;
    public UnbanCommand(ICommandContext context, IDiscordRestChannelAPI channels, IInfractionService infractions)
    {
        _context     = context;
        _channels    = channels;
        _infractions = infractions;
    }


    [Command("unban")]
    [RequireContext(ChannelContext.Guild)]
    [RequireDiscordPermission(DiscordPermission.BanMembers)]
    [Description("Un-bans someone from the current server!")]
    public async Task<IResult> UnbanAsync(IUser user, [Greedy] string reason = "Not Given.")
    {
        var infractionResult = await _infractions.UnBanAsync(_context.GuildID.Value, user.ID, _context.User.ID, reason);
        var notified         = infractionResult.IsDefined(out var result) && result.UserNotified ? "(User notified via DM)" : "(Failed to DM)";
        
        
        return await _channels.CreateMessageAsync
            (
             _context.ChannelID,
             !infractionResult.IsSuccess
                 ? infractionResult.Error.Message
                 : $"{Emojis.WrenchEmoji} Unbanned **{user.ToDiscordTag()}**! {notified}");
    }
}