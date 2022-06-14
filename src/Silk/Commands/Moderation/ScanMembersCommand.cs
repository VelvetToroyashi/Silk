using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Results;
using Silk.Services.Guild;
using Silk.Shared.Constants;
using Silk.Utilities.HelpFormatter;

namespace Silk.Commands.Moderation;

[Category(Categories.Mod)]
public class ScanMembersCommand : CommandGroup
{
    private readonly ICommandContext        _context;
    private readonly MemberScannerService   _scanner;
    private readonly IDiscordRestChannelAPI _channels;
    
    public ScanMembersCommand(ICommandContext context, MemberScannerService scanner, IDiscordRestChannelAPI channels)
    {
        _context  = context;
        _scanner  = scanner;
        _channels = channels;
    }

    [Command("scan")]
    [RequireContext(ChannelContext.Guild)]
    [RequireDiscordPermission(DiscordPermission.KickMembers, DiscordPermission.BanMembers)]
    public async Task<IResult> ScanAsync()
    {
        var IDs = await _scanner.GetSuspicousMembersAsync(_context.GuildID.Value, CancellationToken);

        if (!IDs.Any())
            return await _channels.CreateMessageAsync(_context.ChannelID, "It appears your server is clean!");
        
        var buttons = new ActionRowComponent
        (
         new[]
         {
             new ButtonComponent(ButtonComponentStyle.Secondary, "Dump IDs", new PartialEmoji(DiscordSnowflake.New(Emojis.NoteId)), "member-check:dump"),
             new ButtonComponent(ButtonComponentStyle.Success, "Kick Users", new PartialEmoji(DiscordSnowflake.New(Emojis.KickId)), "member-check:kick"),
             new ButtonComponent(ButtonComponentStyle.Danger, "Ban Users", new PartialEmoji(DiscordSnowflake.New(Emojis.BanId)), "member-check:ban")
         }
        );

        return await _channels.CreateMessageAsync
        (
         _context.ChannelID,
         $"There appears to be {IDs.Count} user{(IDs.Count > 1 ? 's' : null)} detected as phishing.", 
         components: new[] {buttons}
        );
    }
}