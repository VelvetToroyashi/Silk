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
public class NoteCommand : CommandGroup
{
    private readonly ICommandContext     _context;
    private readonly IDiscordRestChannelAPI _channels;
    private readonly IInfractionService  _infractions;
    public NoteCommand(ICommandContext context, IDiscordRestChannelAPI channels, IInfractionService infractions)
    {
        _context     = context;
        _channels    = channels;
        _infractions = infractions;
    }

    [Command("note", "202")]
    [RequireContext(ChannelContext.Guild)]
    [RequireDiscordPermission(DiscordPermission.ManageRoles)]
    [SuppressMessage("ReSharper", "RedundantBlankLines", Justification = "Readability")]
    [Description("Add a note to a user's case history. These do not affect automod actions.")]
    public async Task<IResult> NoteAsync
    (
        [NonSelfActionable]
        [Description("The user to add a note to.")]
        IUser user, 
    
        [Greedy]
        [Description("The note to add.")]
        string note
    )
    {
        var infractionResult = await _infractions.AddNoteAsync(_context.GetGuildID(), user.ID, _context.GetUserID(), note);

        return await _channels.CreateMessageAsync
            (
             _context.GetChannelID(),
             !infractionResult.IsSuccess
                 ? infractionResult.Error.Message
                 : $"{Emojis.NoteEmoji} Note recorded for **{user.ToDiscordTag()}**!"
            );
    }
}