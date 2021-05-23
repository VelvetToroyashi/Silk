using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;

namespace Silk.Core.Commands.Tests
{
    [RequireGuild]
    [Aliases("rm")]
    [Group("rolemenu")]
    public partial class RoleMenuCommands : BaseCommandModule
    {
        private const string CreateMethodDescription = "Create a button-based role menu!" +
                                                       "\nEmojis must be wrapped in quotation marks, and seperated by spaces!\n" +
                                                       "\nYou can put a line break in your message with `\\n`." +
                                                       "\nYour message will automatically be appended with role directions!\n" +
                                                       "\n **e.g.** :emoji: -> @Some Role";
        private readonly TimeSpan InteractionTimeout = TimeSpan.FromMinutes(15);

        [Aliases("ci")]
        [Command("create_interactive")]
        [Description("Create a button-base role menu! \nThis one is interactive.")]
        public async Task CreateInteractive(CommandContext ctx)
        {
            InteractivityExtension input = ctx.Client.GetInteractivity();
            InteractivityResult<DiscordMessage> messageInput;
            ComponentInteractionEventArgs buttonInput;
            DiscordInteraction buttonInteraction;
            DiscordFollowupMessageBuilder followupMessageBuilder = new DiscordFollowupMessageBuilder();
            DiscordMessage currentMessage;
            DiscordMessage messagePreview;

            DiscordComponent[] YNC = new DiscordComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Success, $"{ctx.User.Id}|rolemenu confirm", "Yes", emoji: new("✅")),
                new DiscordButtonComponent(ButtonStyle.Danger, $"{ctx.User.Id} rolemenu decline", "No", emoji: new("❌")),
                new DiscordButtonComponent(ButtonStyle.Secondary, $"{ctx.User.Id} rolemenu abort", "Cancel", emoji: new("⚠️"))
            };

            DiscordButtonComponent start = new DiscordButtonComponent(ButtonStyle.Success, $"{ctx.User.Id} rolemenu init", "Start");

            DiscordMessageBuilder builder = new DiscordMessageBuilder()
                .WithContent("Press start to start. This message is valid for 10 minutes, and the role menu setup expires 15 minutes after that.")
                .WithComponents(start);

            currentMessage = await builder.SendAsync(ctx.Channel);
            buttonInput = (await input.WaitForButtonAsync(currentMessage, TimeSpan.FromMinutes(10))).Result;
            buttonInteraction = buttonInput?.Interaction!;

            start.Disabled = true;
            await currentMessage.ModifyAsync(builder);

            if (buttonInput is null) // null = timed out //
            {
                await ctx.RespondAsync($"{ctx.User.Mention} your setup has timed out.");
                return;
            }

            await buttonInput.Interaction.CreateResponseAsync(InteractionResponseType.DefferedMessageUpdate);
            currentMessage = await buttonInteraction.CreateFollowupMessageAsync(followupMessageBuilder.WithContent("All good role menus start with a name. What's this one's?"));

            while (true)
            {
                messageInput = await input.WaitForMessageAsync(m => m.Author == ctx.User, InteractionTimeout);
                if (messageInput.TimedOut)
                {
                    await ctx.RespondAsync($"{ctx.User.Mention} your setup has timed out.");
                    return;
                }
                currentMessage = await buttonInteraction.EditFollowupMessageAsync(currentMessage.Id, new DiscordWebhookBuilder().WithContent("Are you sure?").WithComponents(YNC));
                buttonInput = (await input.WaitForButtonAsync(currentMessage)).Result;

                if (buttonInput is null)
                {
                    await ctx.RespondAsync($"{ctx.User.Mention} your role menu setup has timed out.");
                    return;
                }


                await buttonInput.Interaction.CreateResponseAsync(InteractionResponseType.DefferedMessageUpdate);

                if (buttonInput.Id.EndsWith("decline"))
                {
                    await currentMessage.ModifyAsync(m => m.WithContent("All good role menus start with a name. What's this one's?"));
                    continue;
                }

                if (buttonInput.Id.EndsWith("abort"))
                {
                    await currentMessage.ModifyAsync(m => m.WithContent("Aborted."));
                    return;
                }

                if (buttonInput.Id.EndsWith("confirm"))
                {
                    await messageInput.Result.DeleteAsync();
                }
            }
        }

        [Command]
        [Aliases("c")]
        [Description(CreateMethodDescription)]
        public async Task Create(CommandContext ctx, string message, string emojis, [RemainingText] params DiscordRole[] roles)
        {
            var converter = (IArgumentConverter<DiscordEmoji>) new DiscordEmojiConverter();
            var split = emojis.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var emojiArray = new DiscordEmoji[roles.Length];

            if (!Validate(message, split, roles, out string? reason))
            {
                await ctx.RespondAsync(reason);
                return;
            }

            for (var i = 0; i < roles.Length; i++)
            {
                var e = await converter.ConvertAsync(split[i], ctx);
                if (!e.HasValue)
                {
                    await ctx.RespondAsync($"I couldn't parse {split[i]}. Did you forget to put a space?");
                    return;
                }

                emojiArray[i] = e.Value;
            }

            var unavailable = emojiArray.Where(e => !e.IsAvailable && e.Id is not 0).ToList();

            if (unavailable.Any())
            {
                await ctx.RespondAsync($"One or more emojis is from a server I'm not in!\nNames:{string.Join(", ", unavailable.Select(u => u.GetDiscordName()))}");
                return;
            }

            var buttons = new List<DiscordComponent>(5);
            var chnk = roles.Zip(emojiArray).Chunk(5).OrderBy(l => l.Count).ToList();

            var builder = new DiscordMessageBuilder()
                .WithContent(message.Replace("\\n", "\n") + $"\n{string.Join('\n', chnk.SelectMany(c => c).Select(p => $"{p.Second} -> {p.First.Mention}"))}")
                .WithAllowedMentions(Mentions.None);

            foreach (var chunklist in chnk)
            {
                foreach ((var role, var emoji) in chunklist)
                {
                    if (role.Position >= ctx.Guild.CurrentMember.Hierarchy)
                        throw new InvalidOperationException("Cannot assign role higher or equal to my own role!");
                    if (role.Position > ctx.Member.Hierarchy)
                        throw new InvalidOperationException("Cannot assign role higher than your own!");

                    var e = new DiscordComponentEmoji {Id = emoji.Id, Name = emoji.Name};
                    var b = new DiscordButtonComponent(ButtonStyle.Success, $"{role.Mention}", emoji, emoji: e);
                    buttons.Add(b);
                }
                builder.WithComponents(buttons.ToArray());
                buttons.Clear();
            }
            await builder.SendAsync(ctx.Channel);
        }

        private static bool Validate(string message, IReadOnlyCollection<string> emojis, IReadOnlyCollection<DiscordRole> roles, out string? reason)
        {
            reason = null;
            if (message.Length > 500)
            {
                reason = "Please keep your message under 500 characters.";
                return false;
            }
            if (emojis.Count != roles.Count)
            {
                reason = "You either have too many or too few emojis for those roles. Did you forget to add a space?";
                return false;
            }
            if (roles.Count is 0)
            {
                reason = "You need to specify at least one role.";
                return false;
            }

            if (emojis.Count <= 25) return true;

            reason = "Sorry, but you can only have 25 roles per role menu!";
            return false;
        }
    }

    public static class ChunkExtension
    {
        public static List<List<T>> Chunk<T>(this IEnumerable<T> data, int size) => data
            .Select((x, i) => new {Index = i, Value = x})
            .GroupBy(x => x.Index / size)
            .Select(x => x.Select(v => v.Value).ToList())
            .ToList();
    }
}