using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Humanizer;
using Silk.Core.Data.Models;
using Silk.Core.Services;
using Silk.Extensions;

namespace Silk.Core.SlashCommands
{
    public sealed class RemindersCommand : SlashCommandModule
    {
        private readonly ReminderService _reminds;
        public RemindersCommand(ReminderService reminds) => _reminds = reminds;

        [SlashCommand("reminders", "Display all active reminders!")]
        public Task Reminders(InteractionContext ctx) => new RemindCommands(_reminds).List(ctx);
    }

    [SlashCommandGroup("aaaaaa", "Reminder related commands!")]
    public sealed class RemindCommands : SlashCommandModule
    {
        private readonly ReminderService _reminders;
        public RemindCommands(ReminderService reminders) => _reminders = reminders;

        [SlashCommand("list", "Lists your active reminders!~")]
        public async Task List(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new() {IsEphemeral = true});

            IEnumerable<Reminder>? reminders = await _reminders.GetRemindersAsync(ctx.User.Id);
            if (reminders is null)
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
        public async Task Create(InteractionContext ctx, string time, string reminder) { }
    }
}