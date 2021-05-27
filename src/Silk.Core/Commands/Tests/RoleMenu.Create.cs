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
using MediatR;
using Silk.Core.Data.MediatR.Guilds;
using Silk.Core.Data.MediatR.ReactionRoles;
using Silk.Core.Data.Models;
using Silk.Extensions.DSharpPlus;

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
        private readonly TimeSpan _interactionTimeout = TimeSpan.FromMinutes(15);
        private readonly TimeSpan _userInteractionWaitTimeout = TimeSpan.FromMinutes(10);
        private readonly TimeSpan _messageUserReadWaitDelay = TimeSpan.FromSeconds(6);
        private readonly IMediator _mediator;

        public RoleMenuCommands(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Aliases("ci")]
        [Command("create_interactive")]
        [Description("Create a button-base role menu! \nThis one is interactive.")]
        public async Task CreateInteractive(CommandContext ctx)
        {
            string buttonIdPrefix = $"{ctx.Message.Id}|{ctx.User.Id}|rolemenu|";

            InteractivityExtension input = ctx.Client.GetInteractivity();
            InteractivityResult<DiscordMessage> messageInput;
            InteractivityResult<ComponentInteractionCreateEventArgs> buttonInput;
            DiscordFollowupMessageBuilder followupMessageBuilder = new();
            DiscordMessage currentMessage;
            DiscordMessage roleMenuDiscordMessage = null!;

            string roleMenuTitle = string.Empty;
            StringBuilder roleMenuMessage = new();
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
            buttonInput = await input.WaitForButtonAsync(currentMessage, _userInteractionWaitTimeout);
            start.Disabled = true;
            builder.WithContent("Rolemenu setup in progress.");
            await currentMessage.ModifyAsync(builder);

            if (buttonInput.TimedOut)
            {
                await ctx.RespondAsync($"{ctx.User.Mention} your setup has timed out.");
                return;
            }

            await buttonInput.Result.Interaction.CreateResponseAsync(InteractionResponseType.DefferedMessageUpdate);
            currentMessage = await buttonInput.Result.Interaction.CreateFollowupMessageAsync(followupMessageBuilder.WithContent("All good role menus start with a name. What's this one's?"));


            bool roleMenuNameResult = await GetRoleMenuNameAsync();

            if (!roleMenuNameResult) return;

            await Task.Delay(_messageUserReadWaitDelay);

            var econ = (IArgumentConverter<DiscordEmoji>) new DiscordEmojiConverter();
            var rcon = (IArgumentConverter<DiscordRole>) new DiscordRoleConverter();

            while (true)
            {
                await currentMessage.ModifyAsync(m => m.WithContent("What would you like to do?").WithComponents(roleMenuOptionsTop).WithComponents(roleMenuOptionsBottom));
                buttonInput = await input.WaitForButtonAsync(currentMessage, _userInteractionWaitTimeout);

                if (buttonInput.TimedOut)
                {
                    await SendTimeoutMessageAsync();
                    return;
                }

                await buttonInput.Result.Interaction.CreateResponseAsync(InteractionResponseType.DefferedMessageUpdate);

                if (buttonInput.Result.Id.EndsWith("abort"))
                    await currentMessage.ModifyAsync(m => m.WithContent("Aborted."));

                if (buttonInput.Result.Id.EndsWith("remove_option"))
                    await RemoveOptionAsync();

                if (buttonInput.Result.Id.EndsWith("preview"))
                    await PreviewRoleMenuAsync();

                if (buttonInput.Result.Id.EndsWith("add_option"))
                    await TryAddOptionAsync();

                if (buttonInput.Result.Id.EndsWith("publish"))
                {
                    bool messagePublished = await ShowPublishAsync();
                    if (!messagePublished)
                        continue;

                    await currentMessage.ModifyAsync(m => m.WithContent($"Congratulations! Your role menu is set up. You can find it here: \n{roleMenuDiscordMessage.JumpLink}"));


                    GuildConfig config = await _mediator.Send(new GetGuildConfigRequest(ctx.Guild.Id));
                    await _mediator.Send(new AddRoleMenuRequest(config.Id, roleMenuDiscordMessage.Id, null!));

                    return;
                }

                if (buttonInput.Result.Id.EndsWith("update_option"))
                    await UpdateOptionAsync();
            }

            async Task UpdateOptionAsync()
            {
                while (true)
                {
                    // I could've used buttons here, but I'm lazy. //
                    await currentMessage.ModifyAsync(m => m.WithContent("Please format your message as such: `<old emoji> <old role> <new emoji> <new role>`! Or type cancel to cancel."));
                    messageInput = await input.WaitForMessageAsync(m => m.Author == ctx.User);
                    if (messageInput.TimedOut)
                    {
                        await currentMessage.ModifyAsync("Sorry, but you took too long!");
                        await Task.Delay(_messageUserReadWaitDelay / 4);
                        return;
                    }

                    if (string.Equals("cancel", messageInput.Result.Content, StringComparison.OrdinalIgnoreCase))
                        return;

                    string[] split = messageInput.Result.Content.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                    if (split.Length is not 4)
                    {
                        await currentMessage.ModifyAsync("Sorry, but you provided incorrect parmeters!");
                        await Task.Delay(_messageUserReadWaitDelay / 4);
                        continue;
                    }

                    string oldemoteraw = split[0];
                    string oldroleraw = split[1];
                    string newemoteraw = split[2];
                    string newroleraw = split[3];

                    Optional<DiscordEmoji> oldEmoji;
                    Optional<DiscordEmoji> newEmoji;

                    Optional<DiscordRole> oldRole;
                    Optional<DiscordRole> newRole;

                    oldEmoji = await econ.ConvertAsync(oldemoteraw, ctx);
                    oldRole = await rcon.ConvertAsync(oldroleraw, ctx);

                    newEmoji = await econ.ConvertAsync(newemoteraw, ctx);
                    newRole = await rcon.ConvertAsync(newroleraw, ctx);

                    if (!await TryCheckValue(oldEmoji, "emoji")) return;
                    if (!await TryCheckValue(newEmoji, "emoji")) return;
                    if (!await TryCheckValue(oldRole, "role")) return;
                    if (!await TryCheckValue(newRole, "role")) return;

                    if (oldEmoji == newEmoji || oldRole == newRole)
                    {
                        await currentMessage.ModifyAsync("New option must be different from old option!");
                        await Task.Delay(_messageUserReadWaitDelay / 4);
                    }
                    else
                    {
                        int index = zipList.IndexOf((oldEmoji.Value, oldRole.Value));
                        if (index is -1)
                        {
                            await currentMessage.ModifyAsync("That option doesn't exist!");
                            await Task.Delay(_messageUserReadWaitDelay / 4);
                        }
                        else
                        {
                            zipList[index] = (newEmoji.Value, newRole.Value);
                            await currentMessage.ModifyAsync("Done!");
                            await Task.Delay(_messageUserReadWaitDelay / 2);
                        }
                    }
                }

                async Task<bool> TryCheckValue<T>(Optional<T> valueContainer, string valueLabel)
                {
                    if (valueContainer.HasValue)
                        return true;

                    await currentMessage.ModifyAsync($"Sorry but that's not a valid {valueLabel}! Please try again.");
                    await Task.Delay(_messageUserReadWaitDelay / 4);
                    return false;
                }
            }

            // Returns whether the menu was published. //
            async Task<bool> ShowPublishAsync()
            {
                await currentMessage.ModifyAsync(m => m.WithContent("Are you sure you want to publish?").WithComponents(yes, no));
                buttonInput = await input.WaitForButtonAsync(currentMessage, ctx.User);

                if (buttonInput.TimedOut)
                {
                    await currentMessage.ModifyAsync(m => m.WithContent("Sorry, but you took too long!"));
                    await Task.Delay(_messageUserReadWaitDelay / 2);
                    return false;
                }

                if (buttonInput.Result.Id.EndsWith("decline"))
                    return false;

                if (buttonInput.Result.Id.EndsWith("confirm"))
                {
                    while (true)
                    {
                        await currentMessage.ModifyAsync(m => m.WithContent("So, what channel would you like to publish to? Alternatively type cancel to cancel."));
                        messageInput = await input.WaitForMessageAsync(m => m.MentionedChannels.Count > 0 ||
                                                                            string.Equals("cancel", m.Content, StringComparison.OrdinalIgnoreCase)
                                                                            && m.Author == ctx.User, _userInteractionWaitTimeout);

                        if (messageInput.TimedOut)
                        {
                            await currentMessage.ModifyAsync("Sorry, but you took to long to respond!");
                            await Task.Delay(_messageUserReadWaitDelay / 4);
                            return false;
                        }

                        if (string.Equals("cancel", messageInput.Result.Content, StringComparison.OrdinalIgnoreCase))
                            return false;

                        DiscordChannel chn = messageInput.Result.MentionedChannels[0];

                        if (chn.Type is not ChannelType.Text)
                        {
                            await currentMessage.ModifyAsync("Sorry, but you can only publish to a text channel!");
                            await Task.Delay(_messageUserReadWaitDelay / 4);
                            continue;
                        }

                        if (!chn.PermissionsFor(ctx.Guild.CurrentMember).HasFlag(Permissions.SendMessages))
                        {
                            await currentMessage.ModifyAsync("I can't send messages to that channel!");
                            await Task.Delay(_messageUserReadWaitDelay / 4);
                            continue;
                        }

                        await currentMessage.ModifyAsync("Alright!");
                        await Task.Delay(_messageUserReadWaitDelay / 4);

                        break;
                    }


                    builder.Clear();

                    builder
                        .WithoutMentions()
                        .WithContent(BuildRoleMenuMessage().ToString());

                    foreach (var chk in buttons.Chunk(5))
                        builder.WithComponents(chk.ToArray());

                    try
                    {
                        roleMenuDiscordMessage = await builder.SendAsync(messageInput.Result.MentionedChannels[0]);
                        return true;
                    }
                    catch (Exception e)
                    {
                        await currentMessage.ModifyAsync($"I'm so sorry! Something went wrong when trying to publish your message. :( \nThe response was `{e.Message}`. Feel free to shoot this to the developers!");
                        return false;
                    }
                }

                return false;

                StringBuilder BuildRoleMenuMessage()
                {
                    roleMenuMessage.Clear();

                    roleMenuMessage.AppendLine(roleMenuTitle);

                    foreach ((DiscordEmoji e, DiscordRole r) in zipList)
                        roleMenuMessage.AppendLine($"{e} **→** {r.Mention}");

                    return roleMenuMessage;
                }
            }

            async Task SendTimeoutMessageAsync()
            {
                await currentMessage.ModifyAsync("Timed out.");
                await ctx.RespondAsync($"{ctx.User.Mention} your rolemenu setup has timed out!");
            }

            async Task RemoveOptionAsync()
            {
                if (!buttons.Any())
                    await buttonInput.Result.Interaction.CreateFollowupMessageAsync(followupMessageBuilder.WithContent("You shouldn't be able to do this!"));

                builder.Clear();
                builder.WithContent($"Alright, what would you like to remove? Or type cancel to cancel. \n{string.Join('\n', zipList.Select((z, i) => $"**{i}**: {z.Item1} → {z.Item2.Mention}"))}");
                var chnk = buttons.Chunk(5);

                foreach (var chunk in chnk)
                    builder.WithComponents(chunk.ToArray());

                await currentMessage.ModifyAsync(builder);

                Task<InteractivityResult<DiscordMessage>> cancelInput = input.WaitForMessageAsync(m => string.Equals("cancel", m.Content, StringComparison.OrdinalIgnoreCase) && m.Author == ctx.User, _userInteractionWaitTimeout);
                Task<InteractivityResult<ComponentInteractionCreateEventArgs>> buttonInputTask = input.WaitForButtonAsync(currentMessage);

                Task inputResult = await Task.WhenAny(cancelInput, buttonInputTask);

                if (inputResult == cancelInput)
                {
                    await currentMessage.ModifyAsync(m => m.WithContent("Alright."));
                    return;
                }

                if (inputResult == buttonInputTask)
                {
                    buttonInput = await buttonInputTask;
                    DiscordComponent componentToRemove = buttons.Single(b => b.CustomId == buttonInput.Result.Id);
                    buttons.Remove(componentToRemove);
                    await currentMessage.ModifyAsync(m => m.WithContent("Alright! Done."));
                    await Task.Delay(_messageUserReadWaitDelay / 2);
                }

                if (buttons.Count is 0)
                {
                    remove.Disabled = true;
                    publish.Disabled = true;
                }
            }

            async Task PreviewRoleMenuAsync()
            {
                List<List<DiscordComponent>> opts = buttons.Chunk(5);

                followupMessageBuilder.Clear();
                followupMessageBuilder.WithContent($"Your menu looks like this so far: \n{roleMenuTitle}\n{roleMenuMessage}\n{string.Join('\n', zipList.Select(z => $"{z.Item1} → {z.Item2.Mention}"))}").AsEphemeral(true);
                foreach (var componentList in opts)
                    followupMessageBuilder.WithComponents(componentList);


                await buttonInput.Result.Interaction.CreateFollowupMessageAsync(followupMessageBuilder);
            }

            async Task TryAddOptionAsync()
            {
                do
                {
                    await currentMessage.ModifyAsync(m => m.WithContent("What would you like to add?\n\n Note: the format`<emoji> <role>`! Place a space in between or I will not add it!"));

                    messageInput = await input.WaitForMessageAsync(m => m.Author == ctx.User);

                    if (messageInput.TimedOut)
                    {
                        await currentMessage.ModifyAsync("Sorry, but you took too long!");
                        return;
                    }

                    string[] rSplit = messageInput.Result.Content.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                    if (rSplit.Length is not 2)
                    {
                        await currentMessage.ModifyAsync("Sorry but you provided an incorrect amount of paremeters! Did you forgot a space?");
                        await Task.Delay(_messageUserReadWaitDelay / 2);
                        continue;
                    }

                    var emojiRaw = rSplit[0];
                    var roleRaw = rSplit[1];

                    var emojiRes = await econ.ConvertAsync(emojiRaw, ctx);
                    var roleRes = await rcon.ConvertAsync(roleRaw, ctx);

                    if (!emojiRes.HasValue)
                    {
                        await currentMessage.ModifyAsync("Sorry but that's not a valid emoji! Please try again.");
                        await Task.Delay(_messageUserReadWaitDelay / 2);
                        continue;
                    }
                    if (!roleRes.HasValue)
                    {
                        await currentMessage.ModifyAsync("Sorry but that's not a valid role! Please try again.");
                        await Task.Delay(_messageUserReadWaitDelay / 2);
                        continue;
                    }
                    if (roleRes.Value.Position >= ctx.Guild.CurrentMember.Hierarchy)
                    {
                        await currentMessage.ModifyAsync("Sorry, but I can't assign that role!");
                        await Task.Delay(_messageUserReadWaitDelay / 2);
                        continue;
                    }
                    if (zipList.Any(r => r.Item1 == emojiRes.Value || r.Item2 == roleRes.Value))
                    {
                        await currentMessage.ModifyAsync("You've already used that role or emoji!");
                        await Task.Delay(_messageUserReadWaitDelay / 2);
                        continue;
                    }
                    if (!emojiRes.Value.IsAvailable && emojiRes.Value.Id is not 0)
                    {
                        await currentMessage.ModifyAsync(m => m.WithContent("That's a custom emote from a server I'm not in! It won't render in the message, but the button will still work. \nDo you want to use it anyway?").WithComponents(yes, no));
                        buttonInput = await input.WaitForButtonAsync(currentMessage, _userInteractionWaitTimeout);

                        if (buttonInput.TimedOut)
                        {
                            await currentMessage.ModifyAsync("Sorry, but you took to long to respond!");
                            await Task.Delay(_messageUserReadWaitDelay / 2);
                            return;
                        }

                        await buttonInput.Result.Interaction.CreateResponseAsync(InteractionResponseType.DefferedMessageUpdate);

                        if (buttonInput.Result.Id.EndsWith("decline"))
                        {
                            await currentMessage.ModifyAsync(m => m.WithContent("Alright then."));
                        }
                        else
                        {
                            await currentMessage.ModifyAsync(m => m.WithContent("Alright!"));
                            await messageInput.Result.DeleteAsync();
                            await Task.Delay(_messageUserReadWaitDelay / 4);
                            zipList.Add((emojiRes.Value, roleRes.Value));
                            buttons.Add(new DiscordButtonComponent(ButtonStyle.Success, $"rolemenu assign {roleRes.Value.Id}", "", emoji: new(emojiRes.Value.Id)));

                            add.Label = $"Add option ({buttons.Count}/25)";

                            remove.Disabled = false;
                            update.Disabled = false;
                            publish.Disabled = false;

                            if (buttons.Count is 25)
                                add.Disabled = true;
                            return;
                        }
                    }
                    else
                    {
                        await currentMessage.ModifyAsync("Alright!");
                        await messageInput.Result.DeleteAsync();
                        await Task.Delay(_messageUserReadWaitDelay / 3);
                        zipList.Add((emojiRes.Value, roleRes.Value));
                        buttons.Add(new DiscordButtonComponent(ButtonStyle.Success, $"rolemenu assign {roleRes.Value.Mention}", "", emoji: new() {Id = emojiRes.Value.Id, Name = emojiRes.Value}));


                        remove.Disabled = false;
                        update.Disabled = false;
                        publish.Disabled = false;
                        add.Label = $"Add option ({buttons.Count}/25)";

                        if (buttons.Count is 25)
                            add.Disabled = true;
                        await Task.Delay(_messageUserReadWaitDelay / 2);
                        return;
                    }
                } while (true);
            }

            // Returns false if the user cancels //
            async Task<bool> GetRoleMenuNameAsync()
            {
                while (true)
                {
                    messageInput = await input.WaitForMessageAsync(m => m.Author == ctx.User, _interactionTimeout);

                    if (messageInput.TimedOut)
                    {
                        await ctx.RespondAsync($"{ctx.User.Mention} your setup has timed out.");
                        return false; // return;
                    }

                    currentMessage = await buttonInput.Result.Interaction.EditFollowupMessageAsync(currentMessage.Id, new DiscordWebhookBuilder().WithContent("Are you sure?").WithComponents(YNC));
                    buttonInput = await input.WaitForButtonAsync(currentMessage);

                    if (buttonInput.TimedOut)
                    {
                        await ctx.RespondAsync($"{ctx.User.Mention} your role menu setup has timed out.");
                        return false; // return;
                    }


                    await buttonInput.Result.Interaction.CreateResponseAsync(InteractionResponseType.DefferedMessageUpdate);

                    if (buttonInput.Result.Id.EndsWith("decline"))
                    {
                        await currentMessage.ModifyAsync(m => m.WithContent("All good role menus start with a name. What's this one's?"));
                        ; // continue;
                    }

                    if (buttonInput.Result.Id.EndsWith("abort"))
                    {
                        await currentMessage.ModifyAsync(m => m.WithContent("Aborted."));
                        return false; // return;
                    }

                    if (!buttonInput.Result.Id.EndsWith("confirm"))
                    {
                        continue; // continue;
                    }

                    roleMenuTitle = messageInput.Result.Content;
                    await messageInput.Result.DeleteAsync();
                    await currentMessage.ModifyAsync(m => m.WithContent("Alright. Got it. Now on to emojis and roles. \n(I will delete your message, so avoid pinging roles in a public channel!)"));
                    return true; // break;   
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