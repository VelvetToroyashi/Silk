using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using Humanizer;
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
        [Command("phishing", "p")]
        [Description("View Anti-Phishing settings for your server.")]
        public async Task<IResult> ViewPhishingAsync()
        {
            var config = await _mediator.Send(new GetGuildModConfig.Request(_context.GuildID.Value));

            var guildResult = await _guilds.GetGuildAsync(_context.GuildID.Value);

            if (!guildResult.IsDefined(out var guild))
                return guildResult;

            //I don't like how long this line is
            var actionType = config.NamedInfractionSteps.TryGetValue(AutoModConstants.PhishingLinkDetected, out var phishingAction) ? phishingAction.Type.Humanize() : "Not configured";

            var enabled = config.DeletePhishingLinks ? Emojis.EnabledEmoji : Emojis.DisabledEmoji;
            var delete  = config.DeletePhishingLinks ? Emojis.EnabledEmoji : Emojis.DisabledEmoji;

            var action = phishingAction is not null ? Emojis.EnabledEmoji : Emojis.DisabledEmoji;

            var embed = new Embed
            {
                Colour = Color.MidnightBlue,
                Title  = $"Phishing detection for {guild.Name}",
                Description = $"{enabled} {Emojis.WarningEmoji} **Detect Phishing Links** \n" +
                              $"{delete} {Emojis.DeleteEmoji} **Delete Phishing Links**  \n"  +
                              $"{action} {Emojis.WrenchEmoji} **After-detection action :** {actionType}"
            };

            return await _channels.CreateMessageAsync(_context.ChannelID, embeds: new[] { embed });
        }
    }
}