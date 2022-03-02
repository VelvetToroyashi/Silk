using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using Remora.Commands.Attributes;
using Remora.Discord.API.Objects;
using Remora.Results;
using Silk.Data.MediatR.Guilds.Config;
using Silk.Extensions;
using Silk.Shared.Constants;

namespace Silk.Commands.Server;

public partial class ConfigCommands
{
    public partial class ViewConfigCommands
    {
        [Command("infractions", "i")]
        [Description("View Infraction settings for your server.")]
        public async Task<IResult> ViewInfractionsAsync()
        {
            var config = await _mediator.Send(new GetGuildModConfig.Request(_context.GuildID.Value));

            var guildResult = await _guilds.GetGuildAsync(_context.GuildID.Value);

            if (!guildResult.IsDefined(out var guild))
                return guildResult;

            var muteRole     = config.MuteRoleID.Value is 0 ? "Not configured." : $"<@&{config.MuteRoleID}>";
            var autoEscalate = config.ProgressiveStriking ? Emojis.EnabledEmoji : Emojis.DisabledEmoji;
            var infractionSteps = !config.InfractionSteps.Any()
                ? "Not configured."
                : config
                 .InfractionSteps
                 .Select((inf, ind) => $"{ind + 1} ➜ {inf.Type.Humanize()}")
                 .Join("\n");

            var infractionStepsNamed = !config.NamedInfractionSteps.Any()
                ? "Not configured."
                : config
                 .NamedInfractionSteps
                 .Select(inf => $"{AutoModConstants.ActionStrings[inf.Key]} ({inf.Key}) ➜ {inf.Value.Type.Humanize()}")
                 .Join("\n");

            var embed = new Embed
            {
                Colour = Color.MidnightBlue,
                Title  = $"Infraction settings for {guild.Name}",
                Description = $"**Mute Role:** {muteRole}\n"                 +
                              $"{autoEscalate} **Automatically Escalate**\n" +
                              $"**Infraction steps:** {infractionSteps}\n"   +
                              $"**Infraction steps (named):** {infractionStepsNamed}"
            };

            return await _channels.CreateMessageAsync(_context.ChannelID, embeds: new[] { embed });
        }
    }
}