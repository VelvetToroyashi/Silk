using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Silk.Core.Database.Models;
using Silk.Core.Utilities;
using Silk.Extensions;
using Silk.Extensions.DSharpPlus;

namespace Silk.Core.Commands.Tests
{
    [Group("role_menu")]
    public class RoleMenuCommand : BaseCommandModule
    {
        [Command("create")]
        [RequireFlag(UserFlag.Staff)]
        public async Task CreateCommand(CommandContext ctx, [RemainingText] params DiscordRole[] roles)
        {
            var builder = new DiscordMessageBuilder();
            if (!roles.Any()) throw new ArgumentException("Must provide roles to create role menu!");
            // Small easter egg for anyone that tries to add @everyone, I guess. Lol. //
            if (ctx.Message.MentionEveryone) // Not that iterating over a couple of elements is expensive, but checking a bool is still faster, and we use that instead. //
            {
                builder.WithContent("Haha, as much as I'd love to add @everyone to @everyone, everyone already has that role!\n" +
                                    "Anyway, let me see what I can do about the rest of the roles :)")
                    .WithReply(ctx.Message.Id, true)
                    .WithoutMentions();
                await ctx.RespondAsync(builder);
            }
            IEnumerable<DiscordRole> higherBotRoles = roles.Where(r => r.Position >= ctx.Guild.CurrentMember.Hierarchy);
            IEnumerable<DiscordRole> higherUserRoles = roles.Where(r => r.Position > ctx.Member.Hierarchy);
            if (higherBotRoles.Any())
            {
                builder.WithContent($"I can't give out {higherBotRoles.Select(r => r.Mention).JoinString(", ")}, as they're higher than me!\n" +
                                    $"The rest of the roles will be put into the menu, however.");
                await ctx.RespondAsync(builder);
                roles = roles.Except(higherBotRoles).ToArray();
            }
            
            if (higherUserRoles.Any())
            {
                builder.WithContent($"I can't give out {higherUserRoles.Select(r => r.Mention).JoinString(", ")}, as they're higher than me!\n" +
                                    "The rest of the roles will be put into the menu, however.");
                await ctx.RespondAsync(builder);
                roles = roles.Except(higherUserRoles).ToArray();
            }
            if (!roles.Any())
            {
                builder.WithContent("Looks like there aren't any roles to add :(");
                await ctx.RespondAsync(builder);
                return;
            }

            InteractivityExtension interactivity = ctx.Client.GetInteractivity();
            builder.WithContent($"So, {roles.Select(r => r.Mention).JoinString(", ")}, right?");

            DiscordMessage confirmationMessage = await ctx.RespondAsync(builder);
            DiscordEmoji confirm = DiscordEmoji.FromGuildEmote(ctx.Client, 799449665346338826);
            DiscordEmoji cancel = DiscordEmoji.FromGuildEmote(ctx.Client, 777724316115796011);
            await confirmationMessage.CreateReactionAsync(confirm);
            await confirmationMessage.CreateReactionAsync(cancel);
            
            InteractivityResult<MessageReactionAddEventArgs> result = 
                await interactivity.WaitForReactionAsync(e => e.Emoji == confirm || e.Emoji == cancel, 
                            confirmationMessage, ctx.User, 
                        TimeSpan.FromSeconds(15));

            if (result.TimedOut)
            {
                builder.WithContent("Canceled. (Timed out).")
                        .WithReply(confirmationMessage.Id);
                await ctx.RespondAsync(builder);
                return;
            }
            if (result.Result.Emoji == cancel)
            {
                builder.WithContent("Canceled. (User-Requested).")
                    .WithReply(confirmationMessage.Id);
                await ctx.RespondAsync(builder);
                return;
            }
            builder.WithReply(default);
            for (int i = 0; i < roles.Length; i++)
            {
                builder.WithReply(0);
            }
            
        }
    }
}