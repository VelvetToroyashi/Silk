using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Humanizer;
using Humanizer.Localisation;
using Silk.Core.Data.Models;
using Silk.Core.Services;
using Silk.Extensions;

namespace Silk.Core.SlashCommands
{
    public sealed class RemindCommands : SlashCommandModule
    {
        [SlashCommandGroup("remind", "Reminder related commands!")]
        public sealed class ReminderCommands : SlashCommandModule
        {
            private readonly ReminderService _reminders;
            public ReminderCommands(ReminderService reminders) => _reminders = reminders;

            [SlashCommand("list", "Lists your active reminders!~")]
            public async Task List(InteractionContext ctx)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new() {IsEphemeral = true});

                Reminder[] reminders = (await _reminders.GetRemindersAsync(ctx.User.Id)).ToArray();

                if (!reminders.Any())
                {
                    await ctx.EditResponseAsync(new() {Content = "Perhaps I'm forgetting something, but you don't seem to have any reminders!"});
                    return;
                }

                string[] allReminders = reminders
                    .Select(r =>
                    {
                        string s = r.Type is ReminderType.Once ?
                            $"`{r.Id}` → Expiring {r.Expiration.Humanize()}:\n" :
                            $"`{r.Id}` → Occurs **{r.Type.Humanize(LetterCasing.LowerCase)}**:\n";

                        if (r.ReplyId is not null)
                            s += $"[reply](https://discord.com/channels/{r.GuildId}/{r.ChannelId}/{r.ReplyId})\n";

                        s += $"`{r.MessageContent}`";
                        return s;
                    })
                    .ToArray();

                string remindersString = allReminders.Join("\n");

                if (remindersString.Length <= 2048)
                {
                    var builder = new DiscordEmbedBuilder();

                    builder.WithColor(DiscordColor.Blurple)
                        .WithTitle($"Reminders for {ctx.User.Username}:")
                        .WithFooter($"Silk! | Requested by {ctx.User.Id}")
                        .WithDescription(remindersString);

                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
                }
                else
                {
                    InteractivityExtension? interactivity = ctx.Client.GetInteractivity();

                    List<Page> pages = allReminders
                        .Select(reminder => new Page("You have too many reminders to fit in one embed, so I've paginated it for you!",
                            new DiscordEmbedBuilder()
                                .WithColor(DiscordColor.Blurple)
                                .WithTitle($"Reminders for {ctx.User.Username}:")
                                .WithDescription(reminder)
                                .WithFooter($"Silk! | Requested by {ctx.User.Id}")))
                        .ToList();
                    await interactivity.SendPaginatedMessageAsync(ctx.Channel, ctx.User, pages);
                }
            }

            [SlashCommand("create", "Create a reminder!")]
            public async Task Create(
                InteractionContext ctx,
                [Option("time", "The time from now you want to be reminded in. Example: 1d12h -> 1 Day, 12 Hours")]
                string time,
                [Option("reminder", "What do you want to be reminded of?")]
                string? reminder)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new() {IsEphemeral = true});
                var conv = (IArgumentConverter<TimeSpan>) new TimeSpanConverter();
                var fctx = ctx.Client.GetCommandsNext().CreateFakeContext(ctx.User, ctx.Channel, ctx.CommandName, "/", null, null);
                var convRes = await conv.ConvertAsync(time, fctx);

                if (!convRes.HasValue)
                {
                    await ctx.EditResponseAsync(new() {Content = "Sorry, but that doesn't appear to be a valid time!"});
                    return;
                }

                await _reminders.CreateReminder(DateTime.UtcNow + convRes.Value, ctx.User.Id, ctx.Channel.Id, 0, 0, reminder);
                await ctx.EditResponseAsync(new() {Content = $"Done. I'll remind you in {convRes.Value.Humanize(3, maxUnit: TimeUnit.Month, minUnit: TimeUnit.Second)}!"});
            }
        }


    }
}