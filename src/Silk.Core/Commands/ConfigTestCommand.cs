using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using MediatR;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Results;
using Silk.Core.Data.Entities;
using Silk.Core.Data.MediatR.Guilds;
using Silk.Core.Data.MediatR.Guilds.Config;

namespace Silk.Core.Commands;

[Group("config")]
[Description("Configure various settings for your guild!")]
//[RequireDiscordPermission(DiscordPermission.ManageGuild)]
public class ConfigTestCommand : CommandGroup
{
    [Group("edit")]
    [Description("Edit various settings for your guild!")]
    public class EditConfig : CommandGroup
    {
        private const string WebhookLoggingName = "Silk! Logging";
            
        private readonly IMediator              _mediator;
        private readonly ICommandContext        _context;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IDiscordRestWebhookAPI _webhookApi;
            
        public EditConfig
        (
            IMediator mediator,
            ICommandContext context,
            IDiscordRestChannelAPI channelApi,
            IDiscordRestWebhookAPI webhookApi
        )
        {
            _mediator = mediator;
            _context = context;
            _channelApi = channelApi;
            _webhookApi = webhookApi;
        }
            
        [Command("logging")]
        [Description("Edit the logging settings for your guild!")]
        [SuppressMessage("ReSharper", "RedundantBlankLines", Justification = "Too many parameters")]
        public async Task<Result> EditLogging
        (
            [Option('f', "fc")]
            [Description("The fallback channel to use for logging.")]
            IChannel? fallbackChannel = null,
                
            [Option('i', "ic")]
            [Description("The channel to use for logging infractions.")]
            IChannel? infractionsChannel = null,
                
            [Option('e', "ec")]
            [Description("The channel to use for logging edits.")]
            IChannel? editsChannel = null,
                
            [Option('d', "dc")]
            [Description("The channel to use for logging deletes.")]
            IChannel? deletesChannel = null,
                
            [Option('j', "jc")]
            [Description("The channel to use for logging joins.")]
            IChannel? joinsChannel = null,
                
            [Option('l', "lc")]
            [Description("The channel to use for logging leaves.")]
            IChannel? leavesChannel = null,
                
            [Option('w', "webhook")]
            [Description("Whether or not to use webhooks for logging.")]
            bool? useWebhooks = null,
                
            [Option("edits")]
            [Description("Whether to log message edits.")]
            bool? logEdits = null,
                
            [Option("deletes")]
            [Description("Whether to log message deletes.")]
            bool? logDeletes = null,
                
            [Option("joins")]
            [Description("Whether to log user joins.")]
            bool? logJoins = null,
                
            [Option("leaves")]
            [Description("Whether to log user leaves.")]
            bool? logLeaves = null,
                
            [Option("infractions")]
            [Description("Whether to log infractions.")]
            bool? logInfractions = null
        )
        {
            var currentConfig = await _mediator.Send(new GetGuildModConfigRequest(_context.GuildID.Value.Value));

            var logging = currentConfig!.LoggingConfig;

            if (logEdits.HasValue)
                logging.LogMessageEdits = logEdits.Value;
                
            if (logDeletes.HasValue)
                logging.LogMessageDeletes = logDeletes.Value;
                
            if (logJoins.HasValue)
                logging.LogMemberJoins = logJoins.Value;
                
            if (logLeaves.HasValue)
                logging.LogMemberLeaves = logLeaves.Value;
                
            if (logInfractions.HasValue)
                logging.LogInfractions = logInfractions.Value;
                
            if (fallbackChannel != null)
                logging.FallbackLoggingChannel = fallbackChannel.ID.Value;

            if (infractionsChannel != null)
                logging.Infractions = await CreateLoggingChannelAsync(useWebhooks, infractionsChannel);

            if (editsChannel != null)
                logging.MessageEdits = await CreateLoggingChannelAsync(useWebhooks, editsChannel);

            if (deletesChannel != null)
                logging.MessageDeletes = await CreateLoggingChannelAsync(useWebhooks, deletesChannel);

            if (joinsChannel != null)
                logging.MemberJoins = await CreateLoggingChannelAsync(useWebhooks, joinsChannel);

            if (leavesChannel != null)
                logging.MemberLeaves = await CreateLoggingChannelAsync(useWebhooks, leavesChannel);
                
            await _mediator.Send(new UpdateGuildModConfigRequest(_context.GuildID.Value.Value)
            {
                LoggingConfig = logging
            });
                
            return Result.FromSuccess();
        }
            
        private async Task<LoggingChannelEntity> CreateLoggingChannelAsync(bool? useWebhooks, IChannel channel)
        {
            if (!useWebhooks ?? false)
            {
                return new()
                {
                    ChannelId = channel.ID.Value,
                    GuildId = channel.GuildID.Value.Value
                };
            }
            else
            {
                var whResult = await _webhookApi.CreateWebhookAsync(channel.ID, WebhookLoggingName, default);

                if (!whResult.IsSuccess)
                {
                    return new()
                    {
                        ChannelId = channel.ID.Value,
                        GuildId = channel.GuildID.Value.Value
                    };
                }
                else
                {
                    var webhook = whResult.Entity;
                    return new()
                    {
                        WebhookId = webhook.ID.Value,
                        WebhookToken = webhook.Token.Value,
                        ChannelId = channel.ID.Value,
                        GuildId = channel.GuildID.Value.Value
                    };
                }
            }
        }
    }
}