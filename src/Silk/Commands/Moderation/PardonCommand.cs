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
public class PardonCommand : CommandGroup
{
    private readonly ICommandContext        _context;
    private readonly IDiscordRestChannelAPI _channels;
    private readonly IInfractionService     _infractions;
    
    public PardonCommand(ICommandContext context, IDiscordRestChannelAPI channels, IInfractionService infractions)
    {
        _context     = context;
        _channels    = channels;
        _infractions = infractions;
    }

    [Command("pardon")]
    [RequireContext(ChannelContext.Guild)]
    [RequireDiscordPermission(DiscordPermission.ManageRoles)]
    [SuppressMessage("ReSharper", "RedundantBlankLines", Justification = "Readability")]
    [Description("Pardons a user from an administered infraction. If a case ID is specified, the case must refer to a strike or esclated strike.")]
    public async Task<IResult> PardonAsync
    (
        [NonSelfActionable]
        [Description("The user to pardon.")]
        IUser user, 
        
        [Option('c', "case")]
        [Description("The infraction case to pardon. If not specified, the last applicable infraction will be pardoned.")]
        int? caseID = null,
        
        [Greedy]
        [Description("The reason the user is being pardoned.")]
        string reason = "Not Given."
    )
    {
        var infractionResult = await _infractions.PardonAsync(_context.GuildID.Value, user.ID, _context.User.ID, caseID, reason);

        var caseMessage = caseID is null
            ? $"Pardoned **{user.ToDiscordTag()}** from their last applicable infraction!"
            : $"Pardoned **{user.ToDiscordTag()}** from case **#{caseID}**!";
        
        return await _channels.CreateMessageAsync
                (
                 _context.ChannelID,
                 !infractionResult.IsSuccess
                     ? infractionResult.Error.Message
                     : $"{Emojis.WrenchEmoji} {caseMessage}"
                );
    }
}