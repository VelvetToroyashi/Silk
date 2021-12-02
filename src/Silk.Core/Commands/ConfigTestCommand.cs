using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using MediatR;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Results;
using Silk.Core.Data.MediatR.Guilds;
using Silk.Core.Data.MediatR.Guilds.Config;

namespace Silk.Core.Commands
{
    [Group("config")]
    [Description("Configure various settings for your guild!")]
    [RequireDiscordPermission(DiscordPermission.ManageGuild)]
    public class ConfigTestCommand : CommandGroup
    {
        [Group("edit")]
        [Description("Edit various settings for your guild!")]
        public class EditConfig : CommandGroup
        {
            private readonly IMediator              _mediator;
            private readonly ICommandContext        _context;
            private readonly IDiscordRestChannelAPI _channelApi;
            public EditConfig(IMediator mediator, ICommandContext context, IDiscordRestChannelAPI channelApi)
            {
                _mediator = mediator;
                _context = context;
                _channelApi = channelApi;
            }

            
            [Command("logging")]
            [Description("Edit the logging settings for your guild!")]
            [SuppressMessage("ReSharper", "RedundantBlankLines", Justification = "Too many parameters")]
            public async Task<Result> EditLogging
            (
                [Option('f', "fc")]
                [Description("The fallback channel to use for logging.")]
                IChannel fallbackChannel = null,
                
                [Option('i', "ic")]
                [Description("The channel to use for logging infractions.")]
                IChannel infractionsChannel = null,
                
                [Option('e', "ec")]
                [Description("The channel to use for logging edits.")]
                IChannel editsChannel = null,
                
                [Option('d', "dc")]
                [Description("The channel to use for logging deletes.")]
                IChannel deletesChannel = null,
                
                [Option('j', "jc")]
                [Description("The channel to use for logging joins.")]
                IChannel joinsChannel = null,
                
                [Option('l', "lc")]
                [Description("The channel to use for logging leaves.")]
                IChannel leavesChannel = null,
                
                [Switch('w', "webhook")]
                [Description("Whether or not to use webhooks for logging.")]
                bool useWebhooks = false,
                
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
                //Send a message to the channel telling the user which options have been specified

                var currentConfig = await _mediator.Send(new GetGuildModConfigRequest(_context.GuildID.Value.Value));
                
                
                await _mediator.Send(new UpdateGuildModConfigRequest(_context.GuildID.Value.Value)
                {
                    
                });
                

                return Result.FromSuccess();
            }
        }
    }
}