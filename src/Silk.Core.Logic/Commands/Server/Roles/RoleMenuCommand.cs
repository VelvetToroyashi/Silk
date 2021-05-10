using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MediatR;
using Silk.Core.Data.MediatR.Guilds;
using Silk.Core.Data.MediatR.ReactionRoles;
using Silk.Core.Discord.Services.Interfaces;
using Silk.Extensions.DSharpPlus;
using Silk.Shared.Constants;

namespace Silk.Core.Logic.Commands.Server.Roles
{
    [Hidden]
    [RequireGuild]
    [Aliases("rm")]
    [Group("rolemenu")]
    [RequireBotPermissions(Permissions.ManageRoles)]
    public class RoleMenuCommand : BaseCommandModule
    {
        private record RoleMenuOption(ulong Role, string EmojiName);

        private readonly IMediator _mediator;
        private readonly IServiceCacheUpdaterService _updater;
        private readonly Regex _comboRegex = new(@".?(<a?:(.+):([0-9]+)>).?(<@&[0-9]+>).?");

        public RoleMenuCommand(IMediator mediator, IServiceCacheUpdaterService updater)
        {
            _mediator = mediator;
            _updater = updater;
        }

        [Command]
        [Description("Automagically configure a role menu based on a message! Must provide message link!\n Supported format: `<emoji> @Role` \n`<emoji> @Role`")]
        public async Task Create(CommandContext ctx, DiscordMessage messageLink)
        {
            var message = await messageLink.Channel.GetMessageAsync(messageLink.Id);
            var matches = _comboRegex.Matches(message.Content);
            var config = await _mediator.Send(new GetGuildConfigRequest(ctx.Guild.Id));

            if (config.RoleMenus.Any(r => r.MessageId == messageLink.Id))
            {
                await ctx.RespondAsync($"That role menu is already set up! use `{ctx.Prefix}rolemenu fix` to fix/update it!");
                return;
            }

            DiscordEmoji loading = DiscordEmoji.FromGuildEmote(ctx.Client, Emojis.LoadingId);
            DiscordEmoji failed = DiscordEmoji.FromGuildEmote(ctx.Client, Emojis.DeclineId);
            DiscordEmoji success = DiscordEmoji.FromGuildEmote(ctx.Client, Emojis.ConfirmId);

            await ctx.RespondAsync("Got it! This should only take a few seconds.");

            DiscordMessageBuilder progressMessageBuilder = new();
            progressMessageBuilder.WithContent($"{loading} Checking message...").WithoutMentions();
            DiscordMessage progressMessage = await ctx.Channel.SendMessageAsync(progressMessageBuilder);

            await Task.Delay(1000);
            var failedOptions = new List<string>();
            var validOptions = new List<RoleMenuOption>();

            for (int i = 0; i < matches.Count; i++)
            {
                string emojiName = matches[i].Groups[2].Value;
                if (message.Reactions.All(r => r.Emoji.Name != emojiName))
                {
                    failedOptions.Add($"{failed} `{matches[i].Groups[1].Value}` was missing a reaction");
                    progressMessageBuilder.WithContent($"{failed} {matches[i].Groups[1].Value} is missing it's reaction! Skipping.");
                    await progressMessageBuilder.ModifyAsync(progressMessage);
                    await Task.Delay(1400);
                    continue;
                }

                var roleId = ulong.Parse(matches[i].Groups[4].Value[3..^1]);

                if (ctx.Guild.GetRole(roleId).Position >= ctx.Guild.CurrentMember.Roles.Last().Position)
                {
                    failedOptions.Add($"{failed} <@&{roleId}>'s position is greater than mine");
                    progressMessageBuilder.WithContent($"{failed} Cannot assign <@&{roleId}> due to heiarchy! Skipping.");
                    await progressMessageBuilder.ModifyAsync(progressMessage);
                    await Task.Delay(1400);
                    continue;
                }

                progressMessageBuilder.WithContent($"{success} I'll give people <@&{roleId}> when they react with {matches[i].Groups[1].Value}!");
                await progressMessageBuilder.ModifyAsync(progressMessage);

                validOptions.Add(new(roleId, emojiName));
                await Task.Delay(1400);
            }


            Dictionary<string, ulong> finalizedRoleMenu = validOptions.ToDictionary(o => o.EmojiName, o => o.Role);
            await _mediator.Send(new AddRoleMenuRequest(config.Id, message.Id, finalizedRoleMenu));

            progressMessageBuilder.WithContent("Done!");
            await progressMessageBuilder.ModifyAsync(progressMessage);

            await Task.Delay(1500);

            if (failedOptions.Any())
            {
                string failedMessages = string.Join('\n', failedOptions);
                progressMessageBuilder.WithContent($"There were some issues while setting up one or more reaction roles! Use `{ctx.Prefix}rolemenu fix` after you fix them!\n{failedMessages}");
                await progressMessageBuilder.ModifyAsync(progressMessage);
            }

            _updater.UpdateGuild(ctx.Guild.Id);
        }
    }
}