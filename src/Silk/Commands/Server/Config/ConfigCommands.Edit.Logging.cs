using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Remora.Commands.Attributes;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Data.MediatR.Guilds;
using Silk.Data.MediatR.Guilds.Config;
using Silk.Shared.Constants;

namespace Silk.Commands.Server;

public partial class ConfigCommands
{
    public partial class EditConfigCommands
    {
        [Command("logging")]
        [Description
            (
                "Adjust the settings for logging. \n"                    +
                "If removing options, true or false can be specified.\n" +
                "If a channel is already specified for the action, it will be overridden with the new one."
            )]
        public async Task<IResult> LoggingAsync
        (
            [Option('j', "joins")] [Description("Whether to log when a user joins. ")]                    bool? logJoins   = null,
            [Option('l', "leaves")] [Description("Whether to log when a user leaves.")]                   bool? logLeaves  = null,
            [Option('d', "deletes")] [Description("Whether to log message deletes.")]                     bool? logDeletes = null,
            [Option('e', "edits")] [Description("Whether to log message edits.")]                         bool? logEdits   = null,
            [Option('w', "webhook")] [Description("Whether to log to a webhook.")]                        bool? webhook    = null,
            [Switch('r', "remove")] [Description("Removes specified options from the logging settings.")] bool  remove     = false,
            [Option('c', "channel")] [Description("The channel to log to. This is required if not removing options.")]
            IChannel? channel = null
        )
        {
            if (!remove && channel is null && webhook is null)
            {
                await _channels.CreateReactionAsync(_context.ChannelID, _context.MessageID, $"_:{Emojis.DeclineId}");
                return await _channels.CreateMessageAsync(_context.ChannelID, "`--channel` or `--webhook` is must be specified.");
            }

            var modConfig = await _mediator.Send(new GetGuildModConfig.Request(_context.GuildID.Value));

            var loggingConfig = modConfig.Logging;

            if (remove)
            {
                if (logJoins is not null)
                {
                    loggingConfig.LogMemberJoins = false;
                    loggingConfig.MemberJoins    = null;
                }

                if (logLeaves is not null)
                {
                    loggingConfig.LogMemberLeaves = false;
                    loggingConfig.MemberLeaves    = null;
                }

                if (logDeletes is not null)
                {
                    loggingConfig.LogMessageDeletes = false;
                    loggingConfig.MessageDeletes    = null;
                }

                if (logEdits is not null)
                {
                    loggingConfig.LogMessageEdits = false;
                    loggingConfig.MessageEdits    = null;
                }

                await _mediator.Send
                    (
                     new UpdateGuildModConfig.Request(_context.GuildID.Value)
                     {
                         LoggingConfig = loggingConfig
                     }
                    );

                return await _channels.CreateReactionAsync(_context.ChannelID, _context.MessageID, $"_:{Emojis.ConfirmId}");
            }

            if (channel is not null)
            {
                if (logJoins is true)
                {
                    loggingConfig.LogMemberJoins = true;

                    var lwt = loggingConfig.MemberJoins?.WebhookToken;
                    var lwi = loggingConfig.MemberJoins?.WebhookID;

                    loggingConfig.MemberJoins = new()
                    {
                        ChannelID    = channel.ID,
                        GuildID      = _context.GuildID.Value,
                        WebhookToken = lwt ?? "",
                        WebhookID    = lwi ?? default(Snowflake)
                    };
                }

                if (logLeaves is true)
                {
                    loggingConfig.LogMemberLeaves = true;

                    var lwt = loggingConfig.MemberLeaves?.WebhookToken;
                    var lwi = loggingConfig.MemberLeaves?.WebhookID;

                    loggingConfig.MemberLeaves = new()
                    {
                        ChannelID    = channel.ID,
                        GuildID      = _context.GuildID.Value,
                        WebhookToken = lwt ?? "",
                        WebhookID    = lwi ?? default(Snowflake)
                    };
                }

                if (logDeletes is true)
                {
                    loggingConfig.LogMessageDeletes = true;

                    var lwt = loggingConfig.MessageDeletes?.WebhookToken;
                    var lwi = loggingConfig.MessageDeletes?.WebhookID;

                    loggingConfig.MessageDeletes = new()
                    {
                        ChannelID    = channel.ID,
                        GuildID      = _context.GuildID.Value,
                        WebhookToken = lwt ?? "",
                        WebhookID    = lwi ?? default(Snowflake)
                    };
                }

                if (logEdits is true)
                {
                    loggingConfig.LogMessageEdits = true;

                    var lwt = loggingConfig.MessageEdits?.WebhookToken;
                    var lwi = loggingConfig.MessageEdits?.WebhookID;

                    loggingConfig.MessageEdits = new()
                    {
                        ChannelID    = channel.ID,
                        GuildID      = _context.GuildID.Value,
                        WebhookToken = lwt ?? "",
                        WebhookID    = lwi ?? default(Snowflake)
                    };
                }
            }


            if (webhook is true && channel is not null)
            {
                var success = true;

                if (logJoins is true)
                {
                    var joinWebhook = await TryCreateWebhookAsync(channel.ID);

                    if (joinWebhook.IsDefined(out var jw))
                        loggingConfig.MemberJoins = new()
                        {
                            WebhookID    = jw.ID,
                            ChannelID    = channel.ID,
                            WebhookToken = jw.Token.Value,
                            GuildID      = _context.GuildID.Value
                        };
                    else success = false;
                }

                if (logLeaves is true)
                {
                    var leaveWebhook = await TryCreateWebhookAsync(channel.ID);

                    if (leaveWebhook.IsDefined(out var lw))
                        loggingConfig.MemberLeaves = new()
                        {
                            WebhookID    = lw.ID,
                            ChannelID    = channel.ID,
                            WebhookToken = lw.Token.Value,
                            GuildID      = _context.GuildID.Value
                        };
                    else success = false;
                }

                if (logDeletes is true)
                {
                    var deleteWebhook = await TryCreateWebhookAsync(channel.ID);

                    if (deleteWebhook.IsDefined(out var dw))
                        loggingConfig.MessageDeletes = new()
                        {
                            WebhookID    = dw.ID,
                            ChannelID    = channel.ID,
                            WebhookToken = dw.Token.Value,
                            GuildID      = _context.GuildID.Value
                        };
                    else success = false;
                }

                if (logEdits is true)
                {
                    var editWebhook = await TryCreateWebhookAsync(channel.ID);

                    if (editWebhook.IsDefined(out var ew))
                        loggingConfig.MessageEdits = new()
                        {
                            WebhookID    = ew.ID,
                            ChannelID    = channel.ID,
                            WebhookToken = ew.Token.Value,
                            GuildID      = _context.GuildID.Value
                        };
                    else success = false;
                }

                if (!success)
                {
                    await _channels.CreateReactionAsync(_context.ChannelID, _context.MessageID, $"_:{Emojis.DeclineId}");

                    return await _channels.CreateMessageAsync(_context.ChannelID, "I couldn't create webhooks in that channel. Check the channel settings to ensure I have `Manage Webhooks` please!");
                }

                await _mediator.Send
                    (
                     new UpdateGuildModConfig.Request(_context.GuildID.Value)
                     {
                         LoggingConfig = loggingConfig
                     }
                    );
            }

            return await _channels.CreateReactionAsync(_context.ChannelID, _context.MessageID, $"_:{Emojis.ConfirmEmoji}");
        }

        private async Task<Result<IWebhook>> TryCreateWebhookAsync(Snowflake channelID)
        {
            var selfResult = await _users.GetCurrentUserAsync();

            if (!selfResult.IsSuccess)
                return Result<IWebhook>.FromError(selfResult.Error);

            var webhooks = await _webhooks.GetChannelWebhooksAsync(channelID);

            if (!webhooks.IsSuccess)
                return Result<IWebhook>.FromError(webhooks.Error);

            //Webhook tokens are always returned (if you have permission), so we need to check if the wh is owned by us.
            var webhook = webhooks.Entity.FirstOrDefault
                (
                 wh =>
                     wh.Type is WebhookType.Incoming &&
                     wh.User.IsDefined(out var user) &&
                     user.ID == selfResult.Entity.ID
                );

            //Return the webhook if it already exists.
            if (webhook is not null)
                return Result<IWebhook>.FromSuccess(webhook);

            var createResult = await _webhooks.CreateWebhookAsync(channelID, "Silk! Logging", default);

            return createResult;
        }
    }
}