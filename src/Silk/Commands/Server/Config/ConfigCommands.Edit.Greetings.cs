using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Results;
using Silk.Data.Entities;
using Silk.Data.MediatR.Guilds;
using Silk.Shared.Constants;

namespace Silk.Commands.Server;

public partial class ConfigCommands
{
    public partial class EditConfigCommands
    {
        [Group("greetings", "greeting", "welcome")]
        public class GreetingCommands : CommandGroup
        {
            private readonly IMediator              _mediator;
            private readonly MessageContext         _context;
            private readonly IDiscordRestChannelAPI _channels;

            public enum GreetOption
            {
                Ignore = GreetingOption.DoNotGreet,
                Join   = GreetingOption.GreetOnJoin,
                Role   = GreetingOption.GreetOnRole,
                //Screen  = GreetingOption.GreetOnScreening
            }

            public GreetingCommands
            (
                IMediator              mediator,
                MessageContext         context,
                IDiscordRestChannelAPI channels
            )
            {
                _mediator = mediator;
                _context  = context;
                _channels = channels;
            }

            [Command("add")]
            [Description("Add a greeting message to a channel.")]
            [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
            public async Task<IResult> AddAsync
            (
                [Description("The channel to add the greeting to.")]                                
                IChannel    channel,
                
                [Description("When to greet the member. Available options are `join` and `role`.")]
                GreetOption option,
                
                [Greedy]
                [Description
                 (
                        "The welcome message to send. \n"                                  +
                        "The following subsitutions are supported:\n"                      +
                        "`{s}` - The name of the server.\n"                                +
                        "`{u}` - The username of the user who joined.\n"                   +
                        "`{@u}` - The mention (@user) of the user who joined.\n\n"         +
                        "Greetings larger than 2000 characters will be placed an embed.\n" +
                        "Embeded greetings do not generate pings for mentioned users/roles."
                 )
                ]
                string greeting,
                
                [Option("role")]
                [Description
                 (
                    "The role to check for. \n" +
                    "This can be an ID (`123456789012345678`), or a mention (`@Role`)."
                 )
                ]
                IRole? role = null
            )
            {
                if (option is GreetOption.Role && role is null)
                {
                    await _channels.CreateMessageAsync(_context.ChannelID, "You must specify a role to check for!");

                    return await _channels.CreateReactionAsync(_context.ChannelID, _context!.MessageID, $"_:{Emojis.DeclineId}");
                }

                var config = await _mediator.Send(new GetGuildConfig.Request(_context.GuildID.Value));

                if (config.Greetings.FirstOrDefault(g => g.ChannelID == channel.ID) is { } existingGreeting)
                {
                    await _channels.CreateMessageAsync
                        (
                         _context.ChannelID,
                         $"There appears to already be a greeting set up for that channel! (ID `{existingGreeting.Id}`)\n\n" +
                         "Consider updating or deleting that greeting instead!"
                        );

                    return await _channels.CreateReactionAsync(_context.ChannelID, _context!.MessageID, $"_:{Emojis.DeclineId}");
                }

                var greetingEntity = new GuildGreetingEntity
                {
                    Message    = greeting,
                    ChannelID  = channel.ID,
                    MetadataID = role?.ID,
                    Option     = (GreetingOption)option
                };

                config.Greetings.Add(greetingEntity);

                await _mediator.Send(new UpdateGuildConfig.Request(_context.GuildID.Value, config.Greetings));

                var message = $"Created greeting with ID `{greetingEntity.Id}`\n\n";

                if (greeting.Length > 2000)
                    message += $"Be warned! This greeting is larger than 2000 characters ({greeting.Length}), and will be placed an embed.";

                await _channels.CreateMessageAsync(_context.ChannelID, message);

                return await _channels.CreateReactionAsync(_context.ChannelID, _context.MessageID, $"_:{Emojis.ConfirmId}");
            }

            [Command("update")]
            [Description("Updates an existing greeting.")]
            public async Task<IResult> UpdateGreetingAsync
            (
                [Description("The ID of the greeting to update.")]
                int GreetingID,
                
                [Option("on")]
                [Description("When to greet the member (`join` or `role`).")]
                GreetOption? option = null,
                
                [Option("role")]
                [Description("The role to check for when greeting")]
                IRole? role = null,
                
                [Option("channel")]
                [Description("The new channel to send greetings to")]
                IChannel? channel = null,
                
                [Greedy]
                [Option("greeting")]
                [Description("The new greeting")]
                string? greeting = null
            )
            {
                var config = await _mediator.Send(new GetGuildConfig.Request(_context.GuildID.Value));

                var greetingEntity = config.Greetings.FirstOrDefault(x => x.Id == GreetingID);

                if (greetingEntity is null)
                {
                    await _channels.CreateMessageAsync(_context.ChannelID, "Could not find a greeting with that ID!");

                    return await _channels.CreateReactionAsync(_context.ChannelID, _context!.MessageID, $"_:{Emojis.DeclineId}");
                }

                if (option is GreetOption.Role && role is null)
                {
                    await _channels.CreateMessageAsync(_context.ChannelID, "You must specify a role to check for!");

                    return await _channels.CreateReactionAsync(_context.ChannelID, _context!.MessageID, $"_:{Emojis.DeclineId}");
                }

                greetingEntity.ChannelID  = channel?.ID ?? greetingEntity.ChannelID;
                greetingEntity.MetadataID = role?.ID    ?? greetingEntity.MetadataID;
                greetingEntity.Message    = greeting    ?? greetingEntity.Message;

                if (option is not null)
                    greetingEntity.Option = (GreetingOption)option;

                await _mediator.Send(new UpdateGuildConfig.Request(_context.GuildID.Value, config.Greetings));

                var message = $"Updated greeting with ID `{greetingEntity.Id}`\n\n";

                if (greeting?.Length > 2000)
                    message += $"Be warned! This greeting is larger than 2000 characters ({greeting.Length}), and will be placed an embed.";

                await _channels.CreateMessageAsync(_context.ChannelID, message);

                return await _channels.CreateReactionAsync(_context.ChannelID, _context.MessageID, $"_:{Emojis.ConfirmId}");
            }

            [Command("delete")]
            [Description("Deletes an existing greeting.")]
            public async Task<IResult> Delete
            (
                [Description("The ID of the greeting to delete.")] int GreetingID
            )
            {
                var config = await _mediator.Send(new GetGuildConfig.Request(_context.GuildID.Value));

                var greetingEntity = config.Greetings.FirstOrDefault(x => x.Id == GreetingID);

                if (greetingEntity is null)
                {
                    await _channels.CreateMessageAsync(_context.ChannelID, "I can't seem to find a greeting with that ID!");

                    return await _channels.CreateReactionAsync(_context.ChannelID, _context!.MessageID, $"_:{Emojis.DeclineId}");
                }

                config.Greetings.Remove(greetingEntity);

                await _mediator.Send(new UpdateGuildConfig.Request(_context.GuildID.Value, config.Greetings));

                await _channels.CreateMessageAsync(_context.ChannelID, $"Deleted greeting with ID `{greetingEntity.Id}`");

                return await _channels.CreateReactionAsync(_context.ChannelID, _context.MessageID, $"_:{Emojis.ConfirmId}");
            }
        }
    }
}