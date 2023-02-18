using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using Remora.Commands.Attributes;
using Remora.Discord.API.Objects;
using Remora.Results;
using Silk.Data.MediatR.Guilds;
using Silk.Shared.Constants;
using Silk.Utilities;

namespace Silk.Commands.Server;

public partial class ConfigCommands
{
    public partial class ViewConfigCommands
    {
        [Command("logging")]
        [Description("View the logging configuration.")]
        public async Task<IResult> ViewLoggingAsync()
        {
            var config = await _mediator.Send(new GetGuildConfig.Request(_context.GuildID.Value));
            var logging = config.Logging;

            var webhookLoggingEnabled = logging.UseWebhookLogging ? Emojis.EnabledEmoji : Emojis.DisabledEmoji;
            
            var logMessageEdits = logging.LogMessageEdits ? Emojis.EnabledEmoji : Emojis.DisabledEmoji;
            var logMessageDeletes = logging.LogMessageDeletes ? Emojis.EnabledEmoji : Emojis.DisabledEmoji;
            
            var logMemberJoins = logging.LogMemberJoins ? Emojis.EnabledEmoji : Emojis.DisabledEmoji;
            var logMemberLeaves = logging.LogMemberLeaves ? Emojis.EnabledEmoji : Emojis.DisabledEmoji;
            
            var logInfractions = logging.LogInfractions ? Emojis.EnabledEmoji : Emojis.DisabledEmoji;

            var editLogChannel = logging.MessageEdits?.ChannelID is null
                    ?  "**Not Configured**"
                    : $"<#{logging.MessageEdits.ChannelID}>";

            var deleteLogChannel = logging.MessageDeletes?.ChannelID is null
                    ?  "**Not Configured**"
                    : $"<#{logging.MessageDeletes.ChannelID}>";
            
            var joinLogChannel = logging.MemberJoins?.ChannelID is null
                ?  "**Not Configured**"
                : $"<#{logging.MemberJoins.ChannelID}>";
            
            var leaveLogChannel = logging.MemberLeaves?.ChannelID is null
                ?  "**Not Configured**"
                : $"<#{logging.MemberLeaves.ChannelID}>";
            
            var infractionLogChannel = logging.Infractions?.ChannelID is null
                ?  "**Not Configured**"
                : $"<#{logging.Infractions.ChannelID}>";

            var useMobileFriendlyLogging = logging.UseMobileFriendlyLogging ? Emojis.EnabledEmoji : Emojis.DisabledEmoji;
            
            var embed = new Embed
            {
                Title = "Logging Configuration",
                Description = $"{webhookLoggingEnabled} Use Webhooks for logging\n" +
                              $"{useMobileFriendlyLogging} Use Mobile Friendly Logging\n\n" +
                              $"{logMessageEdits} Log Message Edits\n"                +
                              $"> Log Channel: {editLogChannel}\n\n"       +
                              $"{logMessageDeletes} Log Message Deletes\n"            +
                              $"> Log Channel: {deleteLogChannel}\n\n"   +
                              $"{logMemberJoins} Log Member Joins\n"                  +
                              $"> Log Channel: {joinLogChannel}\n\n"        +
                              $"{logMemberLeaves} Log Member Leaves\n"                +
                              $"> Log Channel: {leaveLogChannel}\n\n" +
                              $"{logInfractions} Log Infractions\n" +
                              $"> Log Channel: {infractionLogChannel}"  ,
                Colour = Color.Goldenrod
            };

            return await _channels.CreateMessageAsync(_context.GetChannelID(), embeds: new[] { embed });
        }
    }
}