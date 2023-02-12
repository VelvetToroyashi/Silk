using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Humanizer;
using Humanizer.Localisation;
using Remora.Commands.Attributes;
using Remora.Commands.Results;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Pagination.Extensions;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Utilities.HelpFormatter;
using Silk.Extensions;
using Silk.Services.Bot;
using Silk.Services.Guild;
using Silk.Utilities;
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
        """
        The reminder to set.
        Natural language such as "in six hours", or formats
        such as `2d` and `15m` are supported.
        
        You can set your timezone via the `timezone` command.
        Reminders like "tonight at 8PM" will use your local time,
        otherwise UTC is used.
        """;
    
    private const string SilentReminderDescription = 
        """
        Controls whether the reminder is sent silently. 
        Silent reminders still mention you, but unlike normal reminders,
        they do not generate a push notification. Useful for leisurely reminders.
        """;

    private readonly ReminderActionCommands _reminderCommands;

    public ReminderCommands(ReminderActionCommands reminderCommands)
    {
        _reminderCommands = reminderCommands;
    }

    [Command("remind")]
    [ExcludeFromCodeCoverage]
    [Description(ReminderDescription)]
    public Task<IResult> Remind([Greedy] string reminder, [Switch('s', "silent")]bool silent = false) => _reminderCommands.RemindAsync(reminder, silent);
    

    [Group("remind")]
    public class ReminderActionCommands : CommandGroup
    {
        private const string ReminderTimeNotPresent = "It seems you didn't specify a time in your reminder.\n" +
                                                      "I can recognize times like 10m, 5h, 2h30m, and even natural language like 'three hours from now' and 'in 2 days'";

        private readonly TimeSpan _minimumReminderTime = TimeSpan.FromMinutes(0.01);
        
        private readonly ReminderService        _reminders;
        private readonly MessageContext         _context;
        private readonly IDiscordRestChannelAPI _channels;
        private readonly FeedbackService        _interactivity;
        private readonly TimeHelper             _timeHelper;

        
        public ReminderActionCommands
        (
            ReminderService        reminders,
            MessageContext         context,
            IDiscordRestChannelAPI channels,
            FeedbackService        interactivity,
            TimeHelper             timeHelper
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
            string reminder,
            [Switch('s', "silent")]
            [Description(SilentReminderDescription)]
            bool silent = false
        )
        {
            var offset     = await _timeHelper.GetOffsetForUserAsync(_context.GetUserID());
            var timeResult = _timeHelper.ExtractTime(reminder, offset, out reminder);

            if (!timeResult.IsDefined(out var time))
                return await _channels.CreateMessageAsync(_context.GetChannelID(), timeResult.Error!.Message);

            if (time <= TimeSpan.Zero)
                return await _channels.CreateMessageAsync(_context.GetChannelID(), "You can't set a reminder in the past!");
            
            if (time < _minimumReminderTime)
                return await _channels.CreateMessageAsync(_context.GetChannelID(), $"You can't set a reminder less than {_minimumReminderTime.Humanize(minUnit: TimeUnit.Minute)}!");

            Snowflake? guildID = _context.GuildID.HasValue ? _context.GuildID.Value : null;

            _ = _context.Message.ReferencedMessage.IsDefined(out var reply);
            
            var reminderTime = DateTimeOffset.UtcNow + time;
            
            await _reminders.CreateReminderAsync
            (
                 reminderTime,
                 _context.GetUserID(),
                 _context.GetChannelID(),
                 _context.GetMessageID(),
                 guildID,
                 reminder,
                 reply?.Content,
                 reply?.ID, 
                 reply?.Author.ID,
                 silent
            );
            
            return await _channels.CreateMessageAsync(_context.GetChannelID(), $"I'll remind you {reminderTime.ToTimestamp()}!");
        }
        
        [Command("list")]
        [Description("Lists all of your reminders.")]
        public async Task<IResult> ListAsync()
        {
            var reminders = (await _reminders.GetUserRemindersAsync(_context.GetUserID())).OrderBy(r => r.ExpiresAt);

            if (!reminders.Any())
                return await _channels.CreateMessageAsync(_context.GetChannelID(), "You don't have any reminders!");

            if (reminders.Count() > 5)
            {
                var chunkedReminders = reminders.Select
                (r =>
                 {
                     var ret = r.MessageID is null
                         ? $"`[{r.Id}]`"
                         : $"[`[{r.Id}]`](https://discord.com/channels/{r.GuildID?.ToString() ?? "@me"}/{r.ChannelID}/{r.MessageID})";

                     return  ret + $" expiring {r.ExpiresAt.ToTimestamp()}:\n" +
                             $"{r.MessageContent.Truncate(100, "[...]")}" +
                             (r.IsReply ? $"\nReplying to [message](https://discordapp.com/channels/{r.GuildID?.ToString() ?? "@me"}/{r.ChannelID}/{r.ReplyMessageID})" : "");
                    }
                   )
                  .Chunk(5)
                  .Select((rs, i) => new Embed
                   {
                       Title = $"Your Reminders ({i * 5 + 1}-{(i + 1) * 5} out of {reminders.Count()}):",
                       Colour = Color.DodgerBlue,
                       Description = rs.Join("\n\n"),
                   })
                  .ToArray();

                return await _interactivity.SendPaginatedMessageAsync(_context.GetChannelID(), _context.GetUserID(), chunkedReminders);
            }
            else
            {
                var formattedReminders = reminders.Select
                (r =>
                 {
                     var ret = r.MessageID is null
                         ? $"`[{r.Id}]`"
                         : $"[`[{r.Id}]`](https://discord.com/channels/{r.GuildID?.ToString() ?? "@me"}/{r.ChannelID}/{r.MessageID})";

                     return  ret                                          + $" expiring {r.ExpiresAt.ToTimestamp()}:\n" +
                             $"{r.MessageContent.Truncate(100, "[...]")}" +
                             (r.IsReply ? $"\nReplying to [message](https://discordapp.com/channels/{r.GuildID?.ToString() ?? "@me"}/{r.ChannelID}/{r.ReplyMessageID})" : "");
                 }
                );
                
                var embed = new Embed
                {
                    Title       = "Your Reminders:",
                    Colour      = Color.DodgerBlue,
                    Description = formattedReminders.Join("\n\n"),
                };
                
                return await _channels.CreateMessageAsync(_context.GetChannelID(), embeds: new [] {embed});
            }
        }

        [Command("view")]
        [Description("View a specific reminder in full.")]
        public async Task<IResult> ViewAsync
        (
            [Description("The ID of the reminder to view.")]
            int reminderID
        )
        {
            var reminders = (await _reminders.GetUserRemindersAsync(_context.GetUserID())).ToArray();

            if (!reminders.Any())
                return await _channels.CreateMessageAsync(_context.GetChannelID(), "You don't have any active reminders!");

            var reminder = reminders.FirstOrDefault(r => r.Id == reminderID);

            if (reminder is null)
                return await _channels.CreateMessageAsync(_context.GetChannelID(), "You don't have a reminder by that ID!");

            var sb = new StringBuilder();

            sb.AppendLine($"Your reminder (ID `{reminder.Id}`) was set {reminder.CreatedAt.ToTimestamp()}.");

            if (!string.IsNullOrEmpty(reminder.MessageContent))
            {
                sb.AppendLine();
                sb.AppendLine("Your reminder was:");

                sb.AppendLine("> " + reminder.MessageContent.Truncate(1800, "[...]").Replace("\n", "\n> "));
                
            }

            if (reminder.IsReply)
            {
                sb.AppendLine();

                sb.AppendLine($"You replied to [this message](https://discord.com/channels/{reminder.GuildID?.ToString() ?? "@me"}/{reminder.ChannelID}/{reminder.MessageID}).");
                sb.AppendLine("In case it's gone missing (I haven't checked!), the content of the message was:");
                sb.AppendLine("> " + reminder.ReplyMessageContent.Truncate(1800, "[...]").Replace("\n", "\n> "));
            }

            var embed = new Embed { Colour = Color.DodgerBlue, Description = sb.ToString() };

            return await _channels.CreateMessageAsync(_context.GetChannelID(), embeds: new[] { embed });
        }

        [Command("cancel")]
        [Description("Cancels a reminder.")]
        public async Task<IResult> CancelAsync([Description("The ID(s) of the reminder you wish to cancel.")] int[] reminderIDs)
        {
            var reminders = (await _reminders.GetUserRemindersAsync(_context.GetUserID())).Select(r => r.Id).ToArray();

            if (!reminders.Any())
                return await _channels.CreateMessageAsync(_context.GetChannelID(), "You don't have any active reminders!");

            var sb = new StringBuilder();

            sb.Append("Cancelled the following reminders:");
            
            foreach (var reminder in reminderIDs)
            {
                if (!reminders.Contains(reminder))
                    continue;

                sb.Append($"`{reminder}` ");
                
                await _reminders.RemoveReminderAsync(reminder);
            }
            
            return await _channels.CreateMessageAsync(_context.GetChannelID(), sb.ToString());
        }
    }
}