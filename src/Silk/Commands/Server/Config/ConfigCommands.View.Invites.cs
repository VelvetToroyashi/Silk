using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using Remora.Commands.Attributes;
using Remora.Discord.API.Objects;
using Remora.Results;
using Silk.Data.MediatR.Guilds.Config;
using Silk.Shared.Constants;

namespace Silk.Commands.Server;

public partial class ConfigCommands
{
    public partial class ViewConfigCommands
    {
        [Command("invites", "inv")]
        [Description("View Invite settings for your server.")]
        public async Task<IResult> ViewInvitesAsync()
        {
            var config = await _mediator.Send(new GetGuildModConfig.Request(_context.GuildID.Value));

            var guildResult = await _guilds.GetGuildAsync(_context.GuildID.Value);

            if (!guildResult.IsDefined(out var guild))
                return guildResult;

            var enabled = config.Invites.WhitelistEnabled ? Emojis.EnabledEmoji : Emojis.DisabledEmoji;
            var delete  = config.Invites.DeleteOnMatch ? Emojis.EnabledEmoji : Emojis.DisabledEmoji;
            var action  = config.Invites.WarnOnMatch ? Emojis.EnabledEmoji : Emojis.DisabledEmoji;
            var scan    = config.Invites.ScanOrigin ? Emojis.EnabledEmoji : Emojis.DisabledEmoji;
            
            var embed = new Embed
            {
                Colour = Color.MidnightBlue,
                Title  = $"Invite detection for {guild.Name}",
                Description = $"**Enabled:** {enabled}\n"       +
                              $"**Scan Invite Origin**: {scan}" +
                              $"**Delete Invites:** {delete}\n" +
                              $"**Warn On Invite:** {action}"
            };

            return await _channels.CreateMessageAsync(_context.ChannelID, embeds: new[] { embed });
        }
    }
}