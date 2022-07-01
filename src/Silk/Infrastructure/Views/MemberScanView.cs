using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Interactivity;
using Remora.Rest.Core;
using RemoraViewsPOC.Attributes;
using RemoraViewsPOC.Types;
using Silk.Shared.Constants;

namespace Silk.Views;

public record MemberScanView(Optional<bool> Disabled = default) : IView
{
    [Row(1)]
    public ButtonComponent DumpID => new(ButtonComponentStyle.Secondary, "Dump IDs", new PartialEmoji(DiscordSnowflake.New(Emojis.NoteId)), CustomIDHelpers.CreateButtonID("member-check::dump"));
    
    [Row(1)]
    public ButtonComponent Kick => new(ButtonComponentStyle.Success, "Kick Users", new PartialEmoji(DiscordSnowflake.New(Emojis.KickId)), CustomIDHelpers.CreateButtonID("member-check::kick"), IsDisabled: Disabled);
    
    [Row(1)]
    public ButtonComponent Ban => new(ButtonComponentStyle.Danger, "Ban Users",  new PartialEmoji(DiscordSnowflake.New(Emojis.BanId)),  CustomIDHelpers.CreateButtonID("member-check::ban"), IsDisabled: Disabled);
}