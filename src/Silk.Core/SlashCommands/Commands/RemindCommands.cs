using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using FluentAssertions.Common;
using Humanizer;
using Silk.Core.Data.Models;
using Silk.Core.Services.Server;
using Silk.Core.Types;
using Silk.Core.Utilities.Bot;
using Silk.Extensions;
using Silk.Extensions.DSharpPlus;

namespace Silk.Core.SlashCommands.Commands
{
    public sealed class RemindCommands : SlashCommandModule
    {
        [SlashCommandGroup("remind", "Reminder related commands!")]
        public sealed class ReminderCommands : SlashCommandModule
        {
            private readonly ReminderService _reminders;
            private readonly DiscordShardedClient _client;
            public ReminderCommands(ReminderService reminders, DiscordShardedClient client)
            {
                _reminders = reminders;
                _client = client;
            }

            [SlashCommand("list", "Lists your active reminders!~")]
            public async Task List(InteractionContext ctx)
            {
                await ctx.CreateThinkingResponseAsync();

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
                            $"`{r.Id}` → Expiring <t:{r.Expiration.ToDateTimeOffset().ToUniversalTime().ToUnixTimeSeconds()}:R> :\n" :
                            $"`{r.Id}` → Occurs **{r.Type.Humanize(LetterCasing.LowerCase)}**:\n";

                        if (r.ReplyId is not null)
                            s += $"[reply](https://discord.com/channels/{r.GuildId}/{r.ChannelId}/{r.ReplyId})\n";

                        s += $"`{r.MessageContent}`";
                        return s;
                    })
                    .ToArray();

                string remindersString = allReminders.Join("\n");
                
                var builder = new DiscordEmbedBuilder();

                builder.WithColor(DiscordColor.Blurple)
                    .WithTitle($"Reminders for {ctx.User.Username}:")
                    .WithFooter($"Silk! | Requested by {ctx.User.Id}")
                    .WithDescription(remindersString);

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
            }

            private async Task CreateNonRecurringReminderAsync(InteractionContext ctx, string time, string? reminder)
            {
                await ctx.CreateThinkingResponseAsync();

                if (string.IsNullOrEmpty(time))
                {
                    await ctx.EditResponseAsync( new() {Content = "Sorry, but you have to specify a time!"});
                    return;
                }

                if (_client.GetMember(u => u.Id == ctx.User.Id) is null)
                {
                    await ctx.EditResponseAsync(new() {Content = $"Sorry, but I don't share any servers with you! You can contact your server owner to [invite me](https://discord.com/api/oauth2/authorize?client_id={ctx.Client.CurrentApplication.Id}&permissions=502656214&scope=bot%20applications.commands) to this server or your own!"});
                    return;
                }

                var conv = new TimeSpanConverter();
                var convRes = await conv.ConvertAsync(time);

                if (!convRes.HasValue)
                {
                    await ctx.EditResponseAsync(new() {Content = "Sorry, but that doesn't appear to be a valid time!"});
                    return;
                }

                await _reminders.CreateReminder(DateTime.UtcNow + convRes.Value, ctx.User.Id, ctx.Interaction.ChannelId, 0, ctx.Interaction.GuildId, reminder);
                await ctx.EditResponseAsync(new() {Content = $"Done. I'll remind you <t:{(DateTime.UtcNow + convRes.Value).ToDateTimeOffset().ToUnixTimeSeconds()}:R>!"});
            }

            [SlashCommand("cancel", "Cancel a reminder!")]
            public async Task Cancel(InteractionContext ctx, [Option("id", "The id of the reminder ")] long reminderId)
            {
                await ctx.CreateThinkingResponseAsync();
                
                var reminder = await _reminders.GetRemindersAsync(ctx.User.Id);

                if (!reminder.Any() || reminder.All(r => r.Id != reminderId))
                {
                    await ctx.EditResponseAsync(new() {Content = "Sorry, but it doesn't look like you have any reminders by that Id!"});
                    return;
                }
                await _reminders.RemoveReminderAsync((int) reminderId);
                await ctx.EditResponseAsync(new() {Content = "Done."});
            }


            [SlashCommand("create", "Create a reminder! You will be reminded relative to when you set it!")]
            public async Task CreateRecurring(InteractionContext ctx,
                
                [Option("occurence", "How often should I remind you?")]
                ReminderTypeOption type,
                
                [Option("offset", "How long (from now) should I remind you? Ex: 2h40m, 3d, or `now`.")]
                string time,
                
                [Option("reminder", "What do you want to be reminded of?")]
                string reminder)
            {
                if (type is ReminderTypeOption.Once)
                {
                    await CreateNonRecurringReminderAsync(ctx, time!, reminder);
                    return;
                }

                await ctx.CreateThinkingResponseAsync();
                
                TimeSpan ts = TimeSpan.Zero;

                if (!string.Equals("now", time, StringComparison.OrdinalIgnoreCase))
                {
                    var conv = new TimeSpanConverter();
                    Optional<TimeSpan> res = await conv.ConvertAsync(time);

                    if (res.HasValue)
                    {
                        ts = res.Value;
                    }
                    else
                    {
                        await ctx.EditResponseAsync(new() {Content = "Sorry, but that offset doesn't look quite right."});
                        return;
                    }
                }

                await SetReminderAsync(ctx, reminder, (ReminderType) type, ts);
            }

            private async Task SetReminderAsync(InteractionContext ctx, string reminder, ReminderType type, TimeSpan offset)
            {
                DateTime time = type switch
                {
                    ReminderType.Hourly => DateTime.UtcNow + offset + TimeSpan.FromHours(1),
                    ReminderType.Daily => DateTime.UtcNow + offset + TimeSpan.FromDays(1),
                    ReminderType.Weekly => DateTime.UtcNow + offset + TimeSpan.FromDays(7),
                    ReminderType.Monthly => DateTime.UtcNow + offset + TimeSpan.FromDays(30),
                    ReminderType.Once => throw new ArgumentException($"{nameof(ReminderType.Once)} is not a supported type."),
                    _ => throw new ArgumentException($"Unknown value for type of {nameof(ReminderType)}")
                };

                await _reminders.CreateReminder(time, ctx.User.Id, ctx.Interaction.ChannelId, 0, ctx.Interaction.GuildId, reminder, false, type);
                await ctx.FollowUpAsync(new() {Content = $"Alrighty! I'll remind you {type.Humanize(LetterCasing.LowerCase)}: {reminder}", IsEphemeral = true});
            }
        }
    }
}