using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using MediatR;
using Silk.Core.Data.MediatR.Guilds;
using Silk.Core.Data.MediatR.ReactionRoles;
using Silk.Core.Data.Models;
using Silk.Core.Services.Interfaces;
using Silk.Extensions;
using Silk.Extensions.DSharpPlus;
using Silk.Shared.Constants;

namespace Silk.Core.Commands.Server.Roles
{
    [RequireGuild]
    [Aliases("rm")]
    [Group("rolemenu")]
    [RequirePermissions(Permissions.ManageRoles)]
    [Description("Create a reaction-based role menu! Deletion/updating coming soon™")]
    public class RoleMenuCommand : BaseCommandModule
    {
        /// <summary>
        ///     Alan please add details.
        /// </summary>
        /// <param name="Role"></param>
        /// <param name="EmojiName"></param>
        private record RoleMenuOption(ulong Role, string EmojiName);

        private readonly IMediator _mediator;
        private readonly IInputService _input;
        private readonly ICacheUpdaterService _updater;

        public RoleMenuCommand(IMediator mediator, ICacheUpdaterService updater, IInputService input)
        {
            _mediator = mediator;
            _updater = updater;
            _input = input;
        }

        [Command]
        [Description("Automagically configure a role menu based on a message! Provide a message link or emoji role combos (:green_circle: @Some Role).")]
        public async Task Create(CommandContext ctx, DiscordMessage messageLink)
        {
            if (messageLink.Reactions.Count is 0)
            {
                await ctx.RespondAsync("That message doesn't have any reactions!");
                return;
            }

            if (messageLink.MentionedRoles.Count is 0)
            {
                await ctx.RespondAsync("That message doesn't mention any roles!");
                return;
            }

            GuildConfig config = await _mediator.Send(new GetGuildConfigRequest(ctx.Guild.Id));
            bool isExistingRoleMenu = config.RoleMenus.Any(r => r.MessageId == messageLink.Id);

            if (isExistingRoleMenu)
            {
                await ctx.RespondAsync($"That role menu is already set up! use `{ctx.Prefix}rolemenu fix` to fix/update it!");
                return;
            }

            DiscordEmoji failed = DiscordEmoji.FromGuildEmote(ctx.Client, Emojis.DeclineId);
            DiscordEmoji success = DiscordEmoji.FromGuildEmote(ctx.Client, Emojis.ConfirmId);

            StringBuilder sb = new();

            DiscordMessage message = await messageLink.Channel.GetMessageAsync(messageLink.Id);
            DiscordMessageBuilder builder = new();
            builder.WithoutMentions();

            builder.WithContent("Got it! This should only take a few seconds.");
            DiscordMessage progressMessage = await builder.SendAsync(ctx.Channel);
            List<RoleMenuOption> options = new();

            for (var i = 0; i < message.Reactions.Count; i++)
            {
                if (i >= message.MentionedRoles.Count) break;

                if (message.MentionedRoles[i].Position >= ctx.Guild.CurrentMember.Roles.Last().Position)
                {
                    sb.AppendLine($"{failed} {message.MentionedRoles[i].Mention} is unavailable due to heiarchy");
                    builder.WithContent(sb.ToString());
                    await progressMessage.ModifyAsync(builder);
                }
                else
                {
                    options.Add(new(message.MentionedRoles[i].Id, message.Reactions[i].Emoji.Name));

                    sb.AppendLine($"{success} {message.Reactions[i].Emoji.Name} → {message.MentionedRoles[i].Mention}");
                    builder.WithContent(sb.ToString());

                    await progressMessage.ModifyAsync(builder);
                }

                await Task.Delay(1000);
            }

            await _mediator.Send(new AddRoleMenuRequest(config.Id, message.Id, options.ToDictionary(o => o!.EmojiName, o => o!.Role)));

            builder.WithContent("You should be set! I'll look for reactions and give people their roles. Thank you for choosing Silk! <3");
            await ctx.RespondAsync(builder);

            _updater.UpdateGuild(ctx.Guild.Id);
        }



        [Command("create_interactive")]
        [Description("test?")]
        public async Task Create(CommandContext ctx, string menuName, [RemainingText] params DiscordRole[] roles)
        {
            roles = roles.Distinct().ToArray();
            int maxBotRolePosition = ctx.Guild.CurrentMember.Roles.Max(r => r.Position);
            // I've never been hurt by code before today ~Velvet //
            InteractivityExtension input = ctx.Client.GetInteractivity();
            IEnumerable<DiscordRole> validRoles = roles.Where(r => r.Position < maxBotRolePosition);

            DiscordMessageBuilder builder = new DiscordMessageBuilder().WithoutMentions();

            DiscordEmoji failed = DiscordEmoji.FromGuildEmote(ctx.Client, Emojis.DeclineId);
            DiscordEmoji success = DiscordEmoji.FromGuildEmote(ctx.Client, Emojis.ConfirmId);

            IEnumerable<DiscordRole> invalidRoles = roles.Except(validRoles);


            if (invalidRoles.Count() is not 0)
            {
                builder.WithContent("Something went wrong with the following roles!\n" + invalidRoles.Select(r => $"{failed} {r.Mention} is [higher than] my top role!").Join("\n"));
                await ctx.RespondAsync(builder);
                return;
            }

            RoleMenuOption[] options = new RoleMenuOption[roles.Length];


            DiscordMessage message = await ctx.RespondAsync("...");

            for (var i = 0; i < roles.Length; i++)
            {
                builder.WithContent($"What emoji would you like to use for {roles[i].Mention}?");
                await message.ModifyAsync(builder);
                string? result = await _input.GetStringInputAsync(ctx.User.Id, ctx.Channel.Id, ctx.Guild.Id, TimeSpan.FromMinutes(2));
                if (result is null)
                {
                    await ctx.RespondAsync("You took too long, sorry!");
                    return;
                }

                Result<DiscordEmoji?> emoji = TryGetEmoji(result);
                if (emoji)
                {
                    options[i] = new(roles[i].Id, result);
                }
                else
                {
                    i--;
                    await message.ModifyAsync("That's not a valid emoji. Please try again.");
                    await Task.Delay(2000);
                }
            }

            builder.WithContent($"You picked {options.Select(o => $"{o.EmojiName} → <@&{o.Role}>").Join("\n")}");

            Result<DiscordEmoji?> TryGetEmoji(string name)
            {
                try
                {
                    DiscordClient client = ctx.Client;
                    DiscordEmoji? emote = name switch
                    {
                        _ when DiscordEmoji.TryFromName(client, name, out DiscordEmoji emoji) => emoji,
                        _ when DiscordEmoji.TryFromUnicode(name, out DiscordEmoji emoji) => emoji,
                        _ => null
                    };
                    return new(emote, emote is null);
                }
                catch
                {
                    return new(null, false);
                }
            }
        }


        /// <summary>
        ///     A paradigm ripped straight from functional programming, if you will.
        ///     <see cref="Result{T}" /> represents an operation that returns a result with semantic information about whether said operation succeeded,
        ///     as simply returning null (or the equivalent Result.NoValue) would not be entirely indicative of whether a function failed, or is returning no
        ///     value and succeeded.
        ///     And thus we use <see cref="Result{T}" /> as there is no class that fulfills this role in the BCL.
        /// </summary>
        /// <param name="Value">The value of the result, if any.</param>
        /// <param name="Succeeded">Whether the result succeeded. <paramref name="Value" /> will be null if false.</param>
        /// <typeparam name="T">The type of result to return. Null if <paramref name="Succeeded" /> is false.</typeparam>
        private record Result<T>(T? Value, bool Succeeded, string? Reason = null)
        {
            public static implicit operator T?(Result<T> r)
            {
                return r.Value;
            }
            public static implicit operator bool(Result<T> r)
            {
                return r.Succeeded;
            }
        }
    }
}