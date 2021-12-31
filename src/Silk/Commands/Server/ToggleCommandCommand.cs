﻿//TODO: This
/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MediatR;
using Silk.Data.Entities;
using Silk.Data.MediatR.Guilds;
using Silk.Utilities;
using Silk.Utilities.HelpFormatter;

namespace Silk.Commands.Server;

[HelpCategory(Categories.Server)]
public class ToggleCommandCommand : BaseCommandModule
{
    private readonly IMediator _mediator;
    public ToggleCommandCommand(IMediator mediator) => _mediator = mediator;

    //[Command]
    [RequireGuild]
    
    [Description("Reenable commands :)")]
    public async Task Enable(CommandContext ctx, [RemainingText] string? commands)
    {
        await HandleCommandToggleAsync(ctx, commands, (list, name) =>
        {
            DisabledCommandEntity? c = list.SingleOrDefault(co => co.CommandName == name);
            if (c is not null) list.Remove(c!);
        });

    }

    //[Command]
    [RequireGuild]
    [RequireFlag(UserFlag.EscalatedStaff)]
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    [Description("Don't want someone running pesky commands on your server? You can disable them globaly!")]
    public async Task Disable(CommandContext ctx, [RemainingText] string? commands)
    {
        await HandleCommandToggleAsync(ctx, commands, (list, name) =>
        {
            var command = new DisabledCommandEntity { CommandName = name, GuildId = ctx.Guild.Id };
            if (!list.Contains(command))
                list.Add(command);
        });
    }

    private async Task HandleCommandToggleAsync(CommandContext ctx, string? commands, Action<List<DisabledCommandEntity>, string> action)
    {

        if (string.IsNullOrEmpty(commands))
        {
            await ctx.RespondAsync("You must specify at least one command!");
            return;
        }

        GuildConfigEntity? config = await _mediator.Send(new GetGuildConfigRequest(ctx.Guild.Id));

        string[]? commandNames = commands.Split(' ');

        CommandsNextExtension cnext = ctx.Client.GetCommandsNext();

        foreach (string commandName in commandNames)
        {
            if (!cnext.RegisteredCommands.ContainsKey(commandName))
                continue;
            action(config.DisabledCommands, commandName);
        }

        await _mediator.Send(new UpdateGuildConfigRequest(ctx.Guild.Id) { DisabledCommands = config.DisabledCommands });

        DiscordEmoji? thumbsUp = DiscordEmoji.FromUnicode("👍");
        await ctx.Message.CreateReactionAsync(thumbsUp);
    }
}*/