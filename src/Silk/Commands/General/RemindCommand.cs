using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Humanizer;
using Humanizer.Localisation;
using Microsoft.Extensions.Logging;

using Recognizers.Text.DateTime.Wrapper;
using Recognizers.Text.DateTime.Wrapper.Models.BclDateTime;
using Remora.Commands.Attributes;
using Remora.Commands.Results;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Interactivity.Services;
using Remora.Discord.Pagination.Extensions;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Utilities.HelpFormatter;
using Silk.Extensions;
using Silk.Services.Bot;
using Silk.Services.Guild;
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
                _    => TimeSpan.Zero
            };
        });
        
        if (returnResult == TimeSpan.Zero)
            return Result<TimeSpan>.FromError(new ParsingError<TimeSpan>(input, "Failed to extract time from input."));
        
        return Result<TimeSpan>.FromSuccess(returnResult);
    }
}

[Category(Categories.General)]
public class ReminderCommands : CommandGroup
{
    private const string ReminderDescription =
        "The reminder to set. A time is required.\n"                                          +
        "You can use natural language like `three hours from now` and `in 2 days`\n"          +
        "If you're accustomed to other bots (or the behavior of V2), you can\n"               +
        "set reminders in the format of `remind 2h30m to go to the gym`.\n\n"                 +
        "Keep in mind that the time will be extrapolated from the first mention of a time.\n" +
        "Time ranges (such as `for 2 days`) are ignored.\n"                                   +
        "Mentions of `in X days` `X hours from now`, and similar are detected.\n\n"           +
        "**NOTE:**: Absolute time (such as `at 5PM`) uses UTC as a reference point.\n"        +
        "It's recommended to use relative time instead (such as `in three hours`).\n"         +
        "`tomorrow` Also works, and is equivalent to 24 hours from now.\n\n"                  +
        "We're aware this is a less-than ideal solution, and hope to add locale support for this in the future. <3";

    private readonly ReminderActionCommands _reminderCommands;

    public ReminderCommands(ReminderActionCommands reminderCommands)
    {
        _reminderCommands = reminderCommands;
    }

    [Command("remind")]
    [ExcludeFromCodeCoverage]
    [Description(ReminderDescription)]
    public Task<IResult> Remind([Greedy] string reminder) => _reminderCommands.RemindAsync(reminder);
    

    [Group("remind")]
    public class ReminderActionCommands : CommandGroup
    {
        private const string ReminderTimeNotPresent = "It seems you didn't specify a time in your reminder.\n" +
                                                      "I can recognize times like 10m, 5h, 2h30m, and even natural language like 'three hours from now' and 'in 2 days'";

        private readonly TimeSpan _minimumReminderTime = TimeSpan.FromMinutes(3);
        
        private readonly ReminderService           _reminders;
        private readonly MessageContext            _context;
        private readonly IDiscordRestChannelAPI    _channels;
        private readonly InteractiveMessageService _interactivity;
        private readonly TimeHelper                _timeHelper;



        public ReminderActionCommands
        (
            ReminderService           reminders,
            MessageContext            context,
            IDiscordRestChannelAPI    channels,
            InteractiveMessageService interactivity,
            TimeHelper                timeHelper
        )
        {
            _context       = context;
            _channels      = channels;
            _reminders     = reminders;
            _interactivity = interactivity;
            _timeHelper    = timeHelper;
        }

        [Command("set", "me", "create")]
        [Description("Reminds you of something in the future.")]
        public async Task<IResult> RemindAsync
        (
            [Greedy]
            [Description(ReminderDescription)]
            string reminder
        )
        {
            var offset     = await _timeHelper.GetOffsetForUserAsync(_context.User.ID);
            var timeResult = _timeHelper.ExtractTime(reminder, offset, out reminder);

            if (!timeResult.IsDefined(out var time))
                return await _channels.CreateMessageAsync(_context.ChannelID, timeResult.Error!.Message);

            if (time <= TimeSpan.Zero)
                return await _channels.CreateMessageAsync(_context.ChannelID, "You can't set a reminder in the past!");
            
            if (time < _minimumReminderTime)
                return await _channels.CreateMessageAsync(_context.ChannelID, $"You can't set a reminder less than {_minimumReminderTime.Humanize(minUnit: TimeUnit.Minute)}!");

            Snowflake? guildID = _context.GuildID.HasValue ? _context.GuildID.Value : null;

            _ = _context.Message.ReferencedMessage.IsDefined(out var reply);
            
            var reminderTime = DateTimeOffset.UtcNow + time;
            
            await _reminders.CreateReminderAsync
            (
             reminderTime,
             _context.User.ID,
             _context.ChannelID,
             _context.MessageID,
             guildID,
             reminder,
             reply?.Content,
             reply?.ID, 
             reply?.Author.ID
            );
            
            return await _channels.CreateMessageAsync(_context.ChannelID, $"I'll remind you {reminderTime.ToTimestamp()}!");
        }
        
        [Command("list")]
        [Description("Lists all of your reminders.")]
        public async Task<IResult> ListAsync()
        {
            var reminders = (await _reminders.GetUserRemindersAsync(_context.User.ID)).OrderBy(r => r.ExpiresAt);

            if (!reminders.Any())
                return await _channels.CreateMessageAsync(_context.ChannelID, "You don't have any reminders!");

            if (reminders.Count() > 5)
            {
                var chunkedReminders = reminders.Select
                   (r => $"`{r.Id}` expiring {r.ExpiresAt.ToTimestamp()}:\n" +
                         $"{r.MessageContent.Truncate(50, "[...]")}"         +
                         (r.IsReply ? $"\nReplying to [message](https://discordapp.com/channels/{r.GuildID}/{r.ChannelID}/{r.ReplyMessageID})" : ""))
                  .Chunk(5)
                  .Select((rs, i) => new Embed
                   {
                       Title = $"Your Reminders ({i * 5 + 1}-{(i + 1) * 5} out of {reminders.Count()}):",
                       Colour = Color.DodgerBlue,
                       Description = rs.Join("\n\n"),
                   })
                  .ToArray();

                return await _interactivity.SendPaginatedMessageAsync(_context.ChannelID, _context.User.ID, chunkedReminders);
            }
            else
            {
                var formattedReminders = reminders.Select
                    (r => $"`{r.Id}` expiring {r.ExpiresAt.ToTimestamp()}:\n" +
                    $"{r.MessageContent.Truncate(50, "[...]")}" +
                    (r.IsReply ? $"\nReplying to [message](https://discordapp.com/channels/{r.GuildID}/{r.ChannelID}/{r.ReplyMessageID})" : ""));

                var embed = new Embed
                {
                    Title       = "Your Reminders:",
                    Colour      = Color.DodgerBlue,
                    Description = formattedReminders.Join("\n\n"),
                };
                
                return await _channels.CreateMessageAsync(_context.ChannelID, embeds: new [] {embed});
            }
        }

        [Command("cancel")]
        [Description("Cancels a reminder.")]
        public async Task<IResult> CancelAsync([Description("The ID(s) of the reminder you wish to cancel.")] int[] reminderIDs)
        {
            var reminders = (await _reminders.GetUserRemindersAsync(_context.User.ID)).Select(r => r.Id).ToArray();

            if (!reminders.Any())
                return await _channels.CreateMessageAsync(_context.ChannelID, "You don't have any active reminders!");

            var sb = new StringBuilder();

            sb.Append("Cancelled the following reminders:");
            
            foreach (var reminder in reminderIDs)
            {
                if (!reminders.Contains(reminder))
                    continue;

                sb.Append($"`{reminder}` ");
                
                await _reminders.RemoveReminderAsync(reminder);
            }
            
            return await _channels.CreateMessageAsync(_context.ChannelID, sb.ToString());
        }
    }
}