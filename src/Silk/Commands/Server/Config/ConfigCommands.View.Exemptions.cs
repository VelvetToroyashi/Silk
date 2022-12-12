using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humanizer;
using Remora.Commands.Attributes;
using Remora.Discord.API.Objects;
using Remora.Results;
using Silk.Data.Entities;
using Silk.Data.MediatR.Guilds;
using Silk.Shared.Constants;
using Silk.Utilities;

namespace Silk.Commands.Server;

public partial class ConfigCommands
{
    public partial class ViewConfigCommands
    {
        private readonly Dictionary<ExemptionCoverage, string> _humanReadableExemptionStrings = new()
        {
            [ExemptionCoverage.AntiInvite] = "Anti-Invite",
            [ExemptionCoverage.AntiPhishing] = "Anti-Phishing",
            [ExemptionCoverage.EditLogging] = "Message Edit Logging",
            [ExemptionCoverage.DeleteLogging] = "Message Delete Logging"
        };

        [Command("exemptions", "exempt")]
        [Description("View Exemption settings for your server.")]
        public async Task<IResult> ViewExemptionsAsync()
        {
            var config = await _mediator.Send(new GetGuildConfig.Request(_context.GuildID.Value));

            var guildResult = await _guilds.GetGuildAsync(_context.GuildID.Value);

            if (!guildResult.IsDefined(out var guild))
                return guildResult;

            if (!config.Exemptions.Any())
                return await _channels.CreateMessageAsync
                (
                     _context.GetChannelID(),
                     $"{Emojis.WarningEmoji} You don't appear to have any configured exemptions! " +
                     "See `help config edit exemptions` to learn how to add them!"
                );

            var sb = new StringBuilder();

            foreach (var exemption in config.Exemptions)
            {
                var mention = exemption.TargetType switch
                {
                    ExemptionTarget.Channel => $"Users in <#{exemption.TargetID}> are",
                    ExemptionTarget.Role    => $"Users with <@&{exemption.TargetID}> are",
                    ExemptionTarget.User    => $"<@{exemption.TargetID}> is",
                    _                       => throw new ArgumentOutOfRangeException()
                };

                
                sb.AppendLine($"{mention} exempt from {exemption.Exemption.Humanize(LetterCasing.Title)}");
                sb.AppendLine();
            }

            var embed = new Embed
            {
                Colour      = Color.Goldenrod,
                Title       = $"Exemption settings for {guild.Name}",
                Description = sb.ToString()
            };

            return await _channels.CreateMessageAsync(_context.GetChannelID(), embeds: new[] { embed });
        }
    }
}