using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MediatR;
using Silk.Core.Utilities;
using Silk.Data.MediatR;
using Silk.Data.Models;

namespace Silk.Core.Commands.Server
{
    public class ToggleCommandCommand : BaseCommandModule
    {
        private readonly IMediator _mediator;
        public ToggleCommandCommand(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Command]
        [RequireGuild]
        [RequireFlag(UserFlag.Staff)]
        [Description("Reenable commands :)")]
        public async Task Enable(CommandContext ctx, [RemainingText] string? commands)
        {
            await HandleCommandToggleAsync(ctx, commands, (list, name) =>
            {
                //It's safe to use Single instead of SingleOrDefault here becase this delegate only gets called if it exists to begin with
                var c = list.Single(co => co.CommandName == name);
                list.Remove(c);
            });

        }
        
        [Command]
        [RequireGuild]
        [RequireFlag(UserFlag.EscalatedStaff)]
        [Description("Don't want someone running pesky commands on your server? You can disable them globaly! Note that sub-commands should be wrapped in quotes.")]
        public async Task Disable(CommandContext ctx, [RemainingText] string? commands)
        {
            await HandleCommandToggleAsync(ctx, commands, (list, name) =>
            {
                var command = new DisabledCommand { CommandName = name, GuildId = ctx.Guild.Id };
                if (!list.Contains(command))
                    list.Add(command);
            });
        }

        private async Task HandleCommandToggleAsync(CommandContext ctx, string? commands, Action<List<DisabledCommand>, string> action)
        {
        
            if (string.IsNullOrEmpty(commands))
            {
                await ctx.RespondAsync("You must specify at least one command!");
                return;
            }

            GuildConfig config = await _mediator.Send(new GuildConfigRequest.Get { GuildId = ctx.Guild.Id });

            var commandNames = commands.Split(' ');
            
            CommandsNextExtension cnext = ctx.Client.GetCommandsNext();
            
            foreach (string commandName in commandNames)
            {
                if (!cnext.RegisteredCommands.ContainsKey(commandName))
                    continue;
                
                // ReSharper disable once SimplifyLinqExpressionUseAll : I know what I'm doing, I think
                if (!config.DisabledCommands.Any(c => c.CommandName == commandName))
                    action(config.DisabledCommands, commandName); 
            }

            await _mediator.Send(new GuildConfigRequest.Update {GuildId = ctx.Guild.Id, DisabledCommands = config.DisabledCommands});

            var thumbsUp = DiscordEmoji.FromUnicode("👍");
            await ctx.Message.CreateReactionAsync(thumbsUp);
        }
        
    }
}