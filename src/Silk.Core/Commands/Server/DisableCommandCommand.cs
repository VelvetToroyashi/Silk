using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Silk.Core.Utilities;
using Silk.Data.MediatR;
using Silk.Data.Models;

namespace Silk.Core.Commands.Server
{
    public class DisableCommandCommand : BaseCommandModule
    {
        private readonly IMediator _mediator;
        private readonly ILogger<DisableCommandCommand> _logger;
        public DisableCommandCommand(IMediator mediator, ILogger<DisableCommandCommand> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [Command]
        [RequireGuild]
        [RequireFlag(UserFlag.Staff)]
        [Description("Don't want someone running pesky commands on your server? You can disable them globaly! Note that sub-commands should be wrapped in quotes.")]
        public async Task Disable(CommandContext ctx, [RemainingText] string commands)
        {
            if (commands is "")
            {
                await ctx.RespondAsync("You must specify at least one command!");
                return;
            }

            _logger.LogTrace("Querying for guild config");
            GuildConfig config = await _mediator.Send(new GuildConfigRequest.Get { GuildId = ctx.Guild.Id });
            _logger.LogTrace("Acquired config; iterating over commands");
            List<string> disabledCommands = commands.Split(' ').ToList();
            
            CommandsNextExtension cnext = ctx.Client.GetCommandsNext();
            foreach (string command in disabledCommands)
            {
                if (command.Contains("disable")) continue;
                if (!cnext.RegisteredCommands.ContainsKey(command))
                {
                    _logger.LogTrace("Invalid command; skipping");
                    continue;
                }
                config.DisabledCommands.Add(new() {CommandName = command});
            }

            await _mediator.Send(new GuildConfigRequest.Update {GuildId = ctx.Guild.Id, DisabledCommands = config.DisabledCommands});

            var thumbsUp = DiscordEmoji.FromUnicode("👍");
            await ctx.Message.CreateReactionAsync(thumbsUp);
        }
    }
}