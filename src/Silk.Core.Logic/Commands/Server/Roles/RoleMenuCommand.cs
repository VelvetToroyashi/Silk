﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MediatR;
using Silk.Core.Data.MediatR.Guilds;
using Silk.Extensions.DSharpPlus;
using Silk.Shared.Constants;

namespace Silk.Core.Logic.Commands.Server.Roles
{
    [Hidden]
    [RequireGuild]
    [Aliases("rm")]
    [Group("rolemenu")]
    [ModuleLifespan(ModuleLifespan.Transient)] // We're gonna hold some states. //
    public class RoleMenuCommand : BaseCommandModule
    {
        private readonly IMediator _mediator;
        private readonly Regex _comboRegex = new(@"((<a?(:.+:)([0-9]+)>).?(<@&[0-9]+>)).?");

        public RoleMenuCommand(IMediator mediator) => _mediator = mediator;

        [Command]
        [Description("Automagically configure a role menu based on a message! Must provide message link!\n Supported format: `<emoji> @Role` \n`<emoji> @Role`")]
        [RequireBotPermissions(Permissions.ManageRoles)]
        public async Task Create(CommandContext ctx, DiscordMessage messageLink)
        {
            var message = await messageLink.Channel.GetMessageAsync(messageLink.Id);
            var matches = _comboRegex.Matches(message.Content);
            var config = await _mediator.Send(new GetGuildConfigRequest(ctx.Guild.Id));

            if (config.ReactionRoles.Any(r => r.MessageId == messageLink.Id))
            {
                await ctx.RespondAsync($"That role menu is already set up! use `{ctx.Prefix}rolemenu fix` to fix/update it!");
                return;
            }

            DiscordEmoji loading = DiscordEmoji.FromGuildEmote(ctx.Client, Emojis.LoadingId);
            DiscordEmoji failed = DiscordEmoji.FromGuildEmote(ctx.Client, Emojis.DeclineId);

            await ctx.RespondAsync("Got it! This should only take a few seconds.");

            DiscordMessageBuilder progressMessageBuilder = new();
            progressMessageBuilder.WithContent($"{loading} Checking message...").WithoutMentions();
            DiscordMessage progressMessage = await ctx.Channel.SendMessageAsync(progressMessageBuilder);

            await Task.Delay(1000);
            var missingReactions = new List<string>();

            for (int i = 0; i <= matches.Count; i++)
            {
                if (message.Reactions.All(r => r.Emoji.Name != matches[i].Groups[3].Value))
                {
                    string emojiName = matches[i].Groups[3].Value;
                    missingReactions.Add(emojiName);
                    progressMessageBuilder.WithContent($"{failed} {emojiName} is missing it's reaction! Skipping.");
                    await progressMessageBuilder.ModifyAsync(progressMessage);
                    await Task.Delay(500);
                    continue;
                }

                progressMessageBuilder.WithContent($"I'll give people {matches[i].Groups[4].Value} when they react with {matches[i].Groups[2].Value}!");
                await progressMessageBuilder.ModifyAsync(progressMessage);
                await Task.Delay(400);

            }








        }
    }
}