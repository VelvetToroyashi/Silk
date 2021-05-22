using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
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

        [Aliases("ci")]
        [Command("create_interactive")]
        [Description("Create a button-base role menu! \nThis one is interactive.")]
        public async Task CreateInteractive(CommandContext ctx)
        {
            var mBuilder = new DiscordMessageBuilder();
            InteractivityExtension input = ctx.Client.GetInteractivity();
            var m = await ctx.RespondAsync("Poggies");

            await Task.Delay(2000);

            var c = new DiscordComponent[] {new DiscordButtonComponent(ButtonStyle.Primary, "a", "Poggers")};

            await m.ModifyAsync(m => m.WithComponentRow(c));


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
                    var b = new DiscordButtonComponent(ButtonStyle.Success, $"{role.Mention}", emoji: e);
                    buttons.Add(b);
                }
                builder.WithComponentRow(buttons.ToArray());
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