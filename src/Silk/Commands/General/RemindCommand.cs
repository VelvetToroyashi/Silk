using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Humanizer;
using Humanizer.Localisation;
using Recognizers.Text.DateTime.Wrapper;
using Recognizers.Text.DateTime.Wrapper.Models.BclDateTime;
using Remora.Commands.Attributes;
using Remora.Commands.Results;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Results;
using Silk.Data.Entities;
using Silk.Utilities.HelpFormatter;
using Silk.Extensions;
using CommandGroup = Remora.Commands.Groups.CommandGroup;

namespace Silk.Commands.General;

public static class MicroTimeParser
{
    private static readonly Regex _timeRegex = new(@"(?<quantity>\d+)(?<unit>mo|[ywdhms])", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static Result<TimeSpan> TryParse(string input)
    {
        var start = TimeSpan.Zero;
        
        var matches = _timeRegex.Matches(input);
        
        if (!matches.Any())
            return Result<TimeSpan>.FromError(new ParsingError<TimeSpan>(input, "Failed to extract time from input."));

        var returnResult = matches.Aggregate(start, (c, n) =>
        {
            var multiplier = int.Parse(n.Groups["quantity"].Value);
            var unit       = n.Groups["unit"].Value;

            return c + unit switch
            {
                "mo" => TimeSpan.FromDays(30  * multiplier),
                "y"  => TimeSpan.FromDays(365 * multiplier),
                "w"  => TimeSpan.FromDays(7   * multiplier),
                "d"  => TimeSpan.FromDays(multiplier),
                "h"  => TimeSpan.FromHours(multiplier),
                "m"  => TimeSpan.FromMinutes(multiplier),
                "s"  => TimeSpan.FromSeconds(multiplier),
                _    => throw new ArgumentOutOfRangeException(nameof(unit), unit, null)
            };
        });
        
        if (returnResult == TimeSpan.Zero)
            return Result<TimeSpan>.FromError(new ParsingError<TimeSpan>(input, "Failed to extract time from input."));
        
        return Result<TimeSpan>.FromSuccess(returnResult);
    }
}

[HelpCategory(Categories.General)]
public class ReminderCommand : CommandGroup
{
    private const string ReminderTimeNotPresent = "It seems you didn't specify a time in your reminder.\n" +
                                                  "I can recognize times like 10m, 5h, 2h30m, and even natural language like 'tomorrow at 5pm' and 'in 2 days'";
    
    private readonly TimeSpan _minimumReminderTime = TimeSpan.FromMinutes(3);
    
    private readonly ICommandContext        _context;
    private readonly IDiscordRestChannelAPI _channels;
    public ReminderCommand(ICommandContext context, IDiscordRestChannelAPI channels)
    {
        _context  = context;
        _channels = channels;
    }
    
    
    
    [Command("remind", "remindme")]
    [Description("Reminds you of something in the future.")]
    public async Task<IResult> RemindAsync
    (
        [Option("cancel")]
        [Description("Cancels a reminder. Mutually exclusive with the other options.")]
        int? cancelID = null,
        
        [Switch("list")]
        [Description("Lists all reminders you have set.")]
        bool list = false,
        
        [Greedy]
        [Description("The reminder to set. A time is required.\n"                                                      +
                     "You can use natural language like `tomorrow at 5pm` and `in 2 days`\n"                           +
                     "If you're accustomed to other bots (or the behavior of V2), you can\n"                           +
                     "set reminders in the format of `remind 2h30m to go to the gym`.\n\n"                             +
                     "Keep in mind that the time will be extrapolated from the first mention of a time.\n"             +
                     "Time ranges (such as `for 2 days`) are ignored.\n"                                               +
                     "Mentions of `in X days` `X hours from now`, and similar are detected.\n\n"                       +
                     "**NOTE:** \nIf you mention a time such as 5pm, it will be interpreted as 5pm UTC.\n"             +
                     "For reference, Noon UTC is <t:1577880000:T>.\n"                                                  +
                     "If the timestamp reads AM, you need to add hours, and the opposite for PM.\n\n"                  +
                     "Because of this, it's advised to not say 'today', as reminders are based on UTC.\n"              +
                     "Your reminder may be rejected for being in the past.\n"                                          +
                     "Instead, use tomorrow, or mention a relative time such as `in five hours`.\n\n"                  +
                     "If you want to set a reminder at a specific time directly, you must interpret the\n"             +
                     "appropriate time based on the timestamp. \n"                                                     +
                     "If it reads 7AM, you're 5 hours behind UTC, and must then remove five hours to your reminder.\n" +
                     "e.g: `remind me tomorrow at 4AM feed the dogs` will remind you at 9AM your time.\n\n"            +
                     "We're aware this is a less-than ideal solution, and hope to add locale support for this in the future. <3")]
        string reminder = ""
    )
    {
        if (string.IsNullOrEmpty(reminder))
            return await _channels.CreateMessageAsync(_context.ChannelID, "You need to specify a reminder!");

        var timeResult = MicroTimeParser.TryParse(reminder.Split(' ')[0]);
        
        if (!timeResult.IsDefined(out var time))
        {
            var parsedTimes = DateTimeV2Recognizer.RecognizeDateTimes(reminder, refTime: DateTime.UtcNow);
            
            if (parsedTimes.FirstOrDefault() is not {} parsedTime || !parsedTime.Resolution.Values.Any())
                return await _channels.CreateMessageAsync(_context.ChannelID, ReminderTimeNotPresent);
            
            var timeModel = parsedTime.Resolution.Values.FirstOrDefault(v => v is DateTimeV2Date or DateTimeV2DateTime);
            
            if (timeModel is null)
                return await _channels.CreateMessageAsync(_context.ChannelID, ReminderTimeNotPresent);

            if (timeModel is DateTimeV2Date vd)
                time = vd.Value - DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(2));
            
            if (timeModel is DateTimeV2DateTime vdt)
                time = vdt.Value - DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(2));
        }
        
        if (time <= TimeSpan.Zero)
            return await _channels.CreateMessageAsync(_context.ChannelID, "You can't set a reminder in the past!");
        
        if (time < _minimumReminderTime)
            return await _channels.CreateMessageAsync(_context.ChannelID, $"You can't set a reminder less than {_minimumReminderTime.Humanize(minUnit: TimeUnit.Minute)}!");
        
        return await _channels.CreateMessageAsync(_context.ChannelID, $"Reminder set for {time.Humanize(minUnit: TimeUnit.Minute, precision: 4)}!");
    }
    
}
/*{
    
    [HelpCategory(Categories.General)]
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

    [RequireGuild]
    [Group("remind")]
    [Aliases("reminder")]
    [HelpCategory(Categories.General)]
    public class RemindCommand : BaseCommandModule
    {
        private readonly ReminderService _reminders;
        public RemindCommand(ReminderService reminders) => _reminders = reminders;

        [GroupCommand]
        [Description("Creates a reminder")]
        public async Task Remind(CommandContext ctx, TimeSpan time, [RemainingText] string reminder)
        {
            ulong? replyId = ctx.Message.ReferencedMessage?.Id;
            ulong? authorId = ctx.Message.ReferencedMessage?.Author?.Id;
            string? replyContent = ctx.Message.ReferencedMessage?.Content;

            await _reminders.CreateReminder(DateTime.UtcNow + time, ctx.User.Id, ctx.Channel.Id,
                                            ctx.Message.Id, ctx.Guild?.Id ?? 0, reminder, ctx.Message.ReferencedMessage is not null, ReminderType.Once, replyId, authorId, replyContent);
            await ctx.RespondAsync($"Alrighty, I'll remind you in {time.Humanize(2, minUnit: TimeUnit.Second)}: {reminder.Pull(..200)}");
        }

        // RECURRING REMINDERS //

        [Command]
        public Task Hourly(CommandContext ctx, TimeSpan offset, [RemainingText] string reminder)
        {
            return CreateRecurringReminder(ctx, reminder, ReminderType.Hourly, offset);
        }


        [Command]
        public Task Hourly(CommandContext ctx, [RemainingText] string reminder)
        {
            return CreateRecurringReminder(ctx, reminder, ReminderType.Hourly, TimeSpan.Zero);
        }

        [Command]
        public Task Daily(CommandContext ctx, TimeSpan offset, [RemainingText] string reminder)
        {
            return CreateRecurringReminder(ctx, reminder, ReminderType.Daily, offset);
        }

        [Command]
        public Task Daily(CommandContext ctx, [RemainingText] string reminder)
        {
            return CreateRecurringReminder(ctx, reminder, ReminderType.Daily, TimeSpan.Zero);
        }

        [Command]
        public Task Weekly(CommandContext ctx, TimeSpan offset, [RemainingText] string reminder)
        {
            return CreateRecurringReminder(ctx, reminder, ReminderType.Weekly, offset);
        }

        [Command]
        public Task Weekly(CommandContext ctx, [RemainingText] string reminder)
        {
            return CreateRecurringReminder(ctx, reminder, ReminderType.Weekly, TimeSpan.Zero);
        }

        [Command]
        public Task Monthly(CommandContext ctx, TimeSpan offset, [RemainingText] string reminder)
        {
            return CreateRecurringReminder(ctx, reminder, ReminderType.Monthly, offset);
        }

        [Command]
        public Task Monthly(CommandContext ctx, [RemainingText] string reminder)
        {
            return CreateRecurringReminder(ctx, reminder, ReminderType.Monthly, TimeSpan.Zero);
        }

        private async Task CreateRecurringReminder(CommandContext ctx, string reminder, ReminderType type, TimeSpan offset)

        {
            DateTime time = type switch
            {
                ReminderType.Hourly  => DateTime.UtcNow + offset + TimeSpan.FromHours(1),
                ReminderType.Daily   => DateTime.UtcNow + offset + TimeSpan.FromDays(1),
                ReminderType.Weekly  => DateTime.UtcNow + offset + TimeSpan.FromDays(7),
                ReminderType.Monthly => DateTime.UtcNow + offset + TimeSpan.FromDays(30),
                ReminderType.Once    => throw new ArgumentException($"{nameof(ReminderType.Once)} is not a supported type."),
                _                    => throw new ArgumentException($"Unknown value for type of {nameof(ReminderType)}")
            };

            await _reminders.CreateReminder(time, ctx.User.Id, ctx.Channel.Id, ctx.Message.Id, ctx.Guild.Id, reminder, false, type);
            await ctx.RespondAsync($"Alrighty! I'll remind you {type.Humanize(LetterCasing.LowerCase)}: {reminder}");
        }


        // NON-RECURRING REMINDERS //
        [Command]
        [Description("Gives you a list of your reminders")]
        public async Task List(CommandContext ctx)
        {
            IEnumerable<ReminderEntity> reminders = await _reminders.GetRemindersAsync(ctx.User.Id);

            if (!reminders.Any())
            {
                await ctx.RespondAsync("You don't have any active reminders!");
            }
            else
            {
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

                    await ctx.RespondAsync(builder);
                }
                else
                {
                    InteractivityExtension? interactivity = ctx.Client.GetInteractivity();

                    List<Page>? pages = allReminders
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
        }

        [Command]
        [Aliases("cancel")]
        [Description("Removes one of your reminders based on the id given")]
        public async Task Remove(CommandContext ctx, int id)
        {
            if ((await _reminders.GetRemindersAsync(ctx.User.Id))?.Any(r => r.Id == id) ?? false)
            {
                await _reminders.RemoveReminderAsync(id);
                await ctx.RespondAsync("Successfully removed reminder.");
            }
            else
            {
                await ctx.RespondAsync("I couldn't find that reminder!");
            }
        }
    }
}*/
