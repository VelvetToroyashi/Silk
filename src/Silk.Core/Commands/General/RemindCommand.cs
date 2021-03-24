using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Humanizer;
using Humanizer.Localisation;
using Silk.Core.Data.Models;
using Silk.Core.Services;
using Silk.Core.Utilities.HelpFormatter;
using Silk.Extensions;

namespace Silk.Core.Commands.General
{
    public class RemindersCommand : BaseCommandModule
    {
        [Command]
        public async Task Reminders(CommandContext ctx)
        {
            DiscordUser? user = ctx.User;
            DiscordChannel? channel = ctx.Channel;
            string? content = ctx.Message.Content;
            string? prefix = ctx.Prefix;
            Command? command = ctx.CommandsNext.FindCommand("remind list", out _);
            CommandContext? fctx = ctx.CommandsNext.CreateFakeContext(user, channel, content, prefix, command);

            await ctx.CommandsNext.ExecuteCommandAsync(fctx);
        }
    }

    [Group("remind")]
    [Aliases("reminder")]
    [Category(Categories.General)]
    public class RemindCommand : BaseCommandModule
    {
        private readonly ReminderService _reminders;
        public RemindCommand(ReminderService reminders)
        {
            _reminders = reminders;
        }

        [GroupCommand]
        [Description("Creates a reminder")]
        public async Task Remind(CommandContext ctx, TimeSpan time, [RemainingText] string reminder)
        {
            ulong? replyId = ctx.Message.ReferencedMessage?.Id;
            ulong? authorId = ctx.Message.ReferencedMessage?.Author?.Id;
            string? replyContent = ctx.Message.ReferencedMessage?.Content;
            
            await _reminders.CreateReminder(DateTime.UtcNow + time, ctx.User.Id, ctx.Channel.Id,
                ctx.Message.Id, ctx.Guild.Id, reminder, ctx.Message.ReferencedMessage is not null, replyId, authorId, replyContent);
            await ctx.RespondAsync($"Alrighty, I'll remind you in {time.Humanize(2, minUnit: TimeUnit.Second)}: {reminder.Pull(..200)}");
        }
        
        [Command]
        [Description("Gives you a list of your reminders")]
        public async Task List(CommandContext ctx)
        {
            IEnumerable<Reminder>? reminders = await _reminders.GetRemindersAsync(ctx.User.Id);

            if(reminders is null)
            {
                await ctx.RespondAsync("You don't have any active reminders!");
            }
            else
            {
                string[] allReminders = reminders
                .Select(r =>
                {
                    var s = $"`{r.Id}` → Expiring {r.Expiration.Humanize()}:\n";
                    if(r.ReplyId is not null)
                    {
                        s += $"[Referenced message](https://discord.com/channels/{r.GuildId}/{r.ChannelId}/{r.ReplyId})\n";
                    }
                    s += $"`{r.MessageContent}`";
                    return s;
                }).ToArray();

                if (string.Join('\n', allReminders).Length <= 2048)
                {
                    var builder = new DiscordEmbedBuilder()
                        .WithColor(DiscordColor.Blurple)
                        .WithTitle($"Reminders for {ctx.User.Username}:")
                        .WithFooter($"Silk! | Requested by {ctx.User.Id}");
                    builder.WithDescription(string.Join('\n', allReminders));
                    await ctx.RespondAsync(builder);
                }
                else
                {
                    var interactivity = ctx.Client.GetInteractivity();

                    var pages = new List<Page>();
                    foreach(var reminder in allReminders)
                    {
                        pages.Add(new Page("", new DiscordEmbedBuilder()
                            .WithColor(DiscordColor.Blurple)
                            .WithTitle($"Reminders for {ctx.User.Username}:")
                            .WithDescription(reminder)
                            .WithFooter($"Silk! | Requested by {ctx.User.Id}")));
                    }
                    await interactivity.SendPaginatedMessageAsync(ctx.Channel, ctx.User, pages);
                }
            }
        }

        [Command]
        [Aliases("cancel")]
        [Description("Removes one of your reminders based on the id given")]
        public async Task Remove(CommandContext ctx, int id)
        {
            if((await _reminders.GetRemindersAsync(ctx.User.Id))?.Any(r => r.Id == id) ?? false)
            {
                await _reminders.RemoveReminderAsync(id);
                await ctx.RespondAsync("Successfully removed reminder.");
            }
            else
            {
                await ctx.RespondAsync("I couldn’t find that reminder!");
            }
        }
    }
}