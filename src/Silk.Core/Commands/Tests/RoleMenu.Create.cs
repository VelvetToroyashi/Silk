using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    [ModuleLifespan(ModuleLifespan.Transient)]
    public partial class RoleMenuCommands : BaseCommandModule
    {
        private const string CreateMethodDescription = "Create a button-based role menu!" +
                                                       "\nEmojis must be wrapped in quotation marks, and seperated by spaces!\n" +
                                                       "\nYou can put a line break in your message with `\\n`." +
                                                       "\nYour message will automatically be appended with role directions!\n" +
                                                       "\n **e.g.** :emoji: -> @Some Role";
        private readonly TimeSpan InteractionTimeout = TimeSpan.FromMinutes(15);
        private readonly TimeSpan UserInteractionWaitTimeout = TimeSpan.FromMinutes(10);
        private readonly TimeSpan MessageUserReadWaitDelay = TimeSpan.FromSeconds(6);

        [Aliases("ci")]
        [Command("create_interactive")]
        [Description("Create a button-base role menu! \nThis one is interactive.")]
        public async Task CreateInteractive(CommandContext ctx)
        {
            string buttonIdPrefix = $"{ctx.Message.Id}|{ctx.User.Id}|rolemenu|";

            InteractivityExtension input = ctx.Client.GetInteractivity();
            InteractivityResult<DiscordMessage> messageInput;
            ComponentInteractionEventArgs buttonInput;
            DiscordInteraction buttonInteraction;
            DiscordFollowupMessageBuilder followupMessageBuilder = new();
            DiscordMessage currentMessage;

            string roleMenuTitle;
            StringBuilder roleMenuMessage = new();
            DiscordMessage messagePreview;
            List<DiscordComponent> buttons = new(25);
            List<(DiscordEmoji, DiscordRole)> zipList = new(25);

            //Only used on the first message.
            DiscordButtonComponent start = new(ButtonStyle.Success, $"{buttonIdPrefix}init", "Start");

            DiscordButtonComponent no = new(ButtonStyle.Danger, $"{buttonIdPrefix}decline", "No", emoji: new("❌"));
            DiscordButtonComponent yes = new(ButtonStyle.Success, $"{buttonIdPrefix}confirm", "Yes", emoji: new("✅"));
            DiscordButtonComponent cancel = new(ButtonStyle.Secondary, $"{buttonIdPrefix}abort", "Cancel", emoji: new("⚠️"));

            DiscordButtonComponent publish = new(ButtonStyle.Success, $"{buttonIdPrefix}publish", "Publish!", true, new("➡️"));
            DiscordButtonComponent preview = new(ButtonStyle.Primary, $"{buttonIdPrefix}preview", "Preview!", emoji: new("📝"));
            DiscordButtonComponent add = new(ButtonStyle.Success, $"{buttonIdPrefix}add_option", "Add option (0/25)", emoji: new("➕"));
            DiscordButtonComponent remove = new(ButtonStyle.Danger, $"{buttonIdPrefix}remove_option", "Remove option", true, new("➖"));
            DiscordButtonComponent update = new(ButtonStyle.Primary, $"{buttonIdPrefix}update_option", "Update option", true, new("🔄"));

            DiscordComponent[] YNC = {yes, no, cancel};
            DiscordComponent[] roleMenuOptionsTop = {publish, preview, cancel};
            DiscordComponent[] roleMenuOptionsBottom = {add, remove, update};


            DiscordMessageBuilder builder = new DiscordMessageBuilder()
                .WithContent("Press start to start. This message is valid for 10 minutes")
                .WithComponents(start);

            currentMessage = await builder.SendAsync(ctx.Channel);
            buttonInput = (await input.WaitForButtonAsync(currentMessage, UserInteractionWaitTimeout)).Result;
            buttonInteraction = buttonInput?.Interaction!;

            start.Disabled = true;
            builder.WithContent("Rolemenu setup in progress.");
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

                if (!buttonInput.Id.EndsWith("confirm")) { continue; }

                roleMenuTitle = messageInput.Result.Content;
                await messageInput.Result.DeleteAsync();
                await currentMessage.ModifyAsync(m => m.WithContent("Alright. Got it. Now on to emojis and roles. \n(I will delete your message, so avoid pinging roles in a public channel!)"));
                break;
            }

            await Task.Delay(MessageUserReadWaitDelay);

            var econ = (IArgumentConverter<DiscordEmoji>) new DiscordEmojiConverter();
            var rcon = (IArgumentConverter<DiscordRole>) new DiscordRoleConverter();

            while (true)
            {
                await currentMessage.ModifyAsync(m => m.WithContent("What would you like to do?").WithComponents(roleMenuOptionsTop).WithComponents(roleMenuOptionsBottom));
                buttonInput = (await input.WaitForButtonAsync(currentMessage, UserInteractionWaitTimeout)).Result;

                if (buttonInput is null)
                {
                    await currentMessage.ModifyAsync("Timed out.");
                    await ctx.RespondAsync($"{ctx.User.Mention} your rolemenu setup has timed out!");
                    return;
                }
                await buttonInput.Interaction.CreateResponseAsync(InteractionResponseType.DefferedMessageUpdate);


                if (buttonInput.Id.EndsWith("abort"))
                {
                    await currentMessage.ModifyAsync("Aborted.");
                    return;
                }


                if (buttonInput.Id.EndsWith("remove_option"))
                {
                    if (!buttons.Any())
                        await buttonInput.Interaction.CreateFollowupMessageAsync(followupMessageBuilder.WithContent("You shouldn't be able to do this!"));

                    //Todo: implement removal.

                }
                if (buttonInput.Id.EndsWith("preview"))
                {
                    var opts = buttons.Chunk(5);

                    followupMessageBuilder.Clear();
                    followupMessageBuilder.WithContent($"Your menu looks like this so far: \n{roleMenuTitle}\n{roleMenuMessage}\n{string.Join('\n', zipList.Select(z => $"{z.Item1} → {z.Item2.Mention}"))}").AsEphemeral(true);
                    foreach (var componentList in opts)
                        followupMessageBuilder.WithComponents(componentList);

                    await buttonInput.Interaction.CreateFollowupMessageAsync(followupMessageBuilder);
                }
                else if (buttonInput.Id.EndsWith("add_option"))
                {
                    await currentMessage.ModifyAsync(m => m.WithContent("What would you like to add?\n\n Note: the format`<emoji> <role>`! Place a space in between or I will not add it!"));

                    do
                    {
                        await Task.Delay(MessageUserReadWaitDelay / 2);
                        messageInput = await input.WaitForMessageAsync(m => m.Author == ctx.User);

                        if (messageInput.TimedOut)
                        {
                            await currentMessage.ModifyAsync("Sorry, but you took too long!");
                            continue;
                        }

                        string[] rSplit = messageInput.Result.Content.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                        if (rSplit.Length is not 2)
                        {
                            await currentMessage.ModifyAsync("Sorry but you provided an incorrect amount of paremeters! Did you forgot a space?");
                            continue;
                        }

                        var emojiRaw = rSplit[0];
                        var roleRaw = rSplit[1];

                        var emojiRes = await econ.ConvertAsync(emojiRaw, ctx);
                        var roleRes = await rcon.ConvertAsync(roleRaw, ctx);

                        if (!emojiRes.HasValue)
                        {
                            await currentMessage.ModifyAsync("Sorry but that's not a valid emoji! Please try again.");
                            continue;
                        }
                        if (!roleRes.HasValue)
                        {
                            await currentMessage.ModifyAsync("Sorry but that's not a valid role! Please try again.");
                            continue;
                        }
                        if (roleRes.Value.Position >= ctx.Guild.CurrentMember.Hierarchy)
                        {
                            await currentMessage.ModifyAsync("Sorry, but I can't assign that role!");
                            continue;
                        }
                        if (zipList.Any(r => r.Item1 == emojiRes.Value || r.Item2 == roleRes.Value))
                        {
                            await currentMessage.ModifyAsync("You've already used that role or emoji!");
                            continue;
                        }
                        if (!emojiRes.Value.IsAvailable && emojiRes.Value.Id is not 0)
                        {
                            await currentMessage.ModifyAsync(m => m.WithContent("That's a custom emote from a server I'm not in! It won't render in the message, but the button will still work. \nDo you want to use it anyway?").WithComponents(yes, no));
                            buttonInput = (await input.WaitForButtonAsync(currentMessage, UserInteractionWaitTimeout)).Result;

                            if (buttonInput is null)
                            {
                                await currentMessage.ModifyAsync("Sorry, but you took to long to respond!");
                                break;
                            }

                            await buttonInput.Interaction.CreateResponseAsync(InteractionResponseType.DefferedMessageUpdate);

                            if (buttonInput.Id.EndsWith("confirm"))
                            {
                                await currentMessage.ModifyAsync(m => m.WithContent("Alright!"));
                                await messageInput.Result.DeleteAsync();
                                await Task.Delay(MessageUserReadWaitDelay / 3);
                                zipList.Add((emojiRes.Value, roleRes.Value));
                                buttons.Add(new DiscordButtonComponent(ButtonStyle.Success, $"rolemenu assign {roleRes.Value.Mention}", "", emoji: new(emojiRes.Value.Name)));

                                add.Label = $"Add option ({buttons.Count}/25)";
                                remove.Disabled = false;
                                update.Disabled = false;
                                publish.Disabled = false;

                                if (buttons.Count is 25)
                                    add.Disabled = true;

                                break;
                            }

                            await currentMessage.ModifyAsync(m => m.WithContent("Alright then."));
                            continue;
                        }


                        await currentMessage.ModifyAsync("Alright!");
                        await messageInput.Result.DeleteAsync();
                        await Task.Delay(MessageUserReadWaitDelay / 3);
                        zipList.Add((emojiRes.Value, roleRes.Value));
                        buttons.Add(new DiscordButtonComponent(ButtonStyle.Success, $"rolemenu assign {roleRes.Value.Mention}", "", emoji: new() {Id = emojiRes.Value.Id, Name = emojiRes.Value.Name}));


                        remove.Disabled = false;
                        update.Disabled = false;
                        publish.Disabled = false;
                        add.Label = $"Add option ({buttons.Count}/25)";

                        if (buttons.Count is 25)
                            add.Disabled = true;

                        break;
                    } while (true);
                }

                else
                {
                    await ctx.Channel.SendMessageAsync("That's not a valid choice! A sign of a client modder, you know.");
                    return;
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