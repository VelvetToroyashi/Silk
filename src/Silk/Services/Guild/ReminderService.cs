using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using Mediator;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus;
using Remora.Discord.API.Abstractions.Gateway.Commands;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Data.Entities;
using Silk.Data.MediatR.Reminders;
using Silk.Extensions;
using Silk.Extensions.Remora;
using Silk.Shared.Constants;
using Silk.Shared.Types;
using Silk.Utilities;
using IMessage = Remora.Discord.API.Abstractions.Objects.IMessage;

namespace Silk.Services.Guild;

public sealed class ReminderService : IHostedService
{
    private readonly IMediator                _mediator;
    private readonly IShardIdentification     _shard;
    private readonly IDiscordRestUserAPI      _users;
    private readonly IDiscordRestChannelAPI   _channels;
    private readonly ILogger<ReminderService> _logger;
    
    // When it comes to sharding, ideally this is only the reminders for the guilds that are in the shard.
    // Perhaps we'll filter manually with `.Where(r => r.GuildID >> 22 % ShardCount == ShardId)`
    private          List<ReminderEntity> _reminders = new(); 
    private readonly AsyncTimer           _timer;

    public ReminderService
    (
        IMediator                mediator,
        IShardIdentification     shard,
        IDiscordRestUserAPI      users,
        IDiscordRestChannelAPI   channels,
        ILogger<ReminderService> logger
    )
    {
        _mediator = mediator;
        _shard    = shard;
        _users    = users;
        _channels = channels;
        _logger   = logger;

        _timer = new(TryDispatchRemindersAsync, TimeSpan.FromSeconds(1), true);
    }

    public async Task CreateReminderAsync
    (
        DateTimeOffset expiry,
        Snowflake      ownerID,
        Snowflake      channelID,
        Snowflake?     messageID,
        Snowflake?     guildID,
        string?        content,
        string?        replyContent  = null,
        Snowflake?     replyID       = null,
        Snowflake?     replyAuthorID = null,
        bool           isSilent = false
    )
    {
        ReminderEntity reminder = await _mediator.Send(new CreateReminder.Request(expiry, ownerID, channelID, messageID, guildID, content, replyID, replyAuthorID, replyContent, isSilent));
        _reminders.Add(reminder);
        SilkMetric.LoadedReminders.Inc();
        _logger.LogDebug("Created reminder {ReminderID}", reminder.Id);
    }

    /// <summary>
    ///     Gets all reminders that are due for a certain user.
    /// </summary>
    /// <param name="userID">The ID of the user to search reminders for.</param>
    /// <returns>The specified user's reminders.</returns>
    public ValueTask<IEnumerable<ReminderEntity>> GetUserRemindersAsync(Snowflake userID) 
        => _mediator.Send(new GetRemindersForUser.Request(userID));

    /// <summary>
    ///     The main dispatch loop, which iterates all active reminders, and dispatches them if they're due.
    /// </summary>
    private async Task TryDispatchRemindersAsync()
    {
        if (!_reminders.Any())
            return;

        DateTime now = DateTime.UtcNow;
        
        var dueReminders = _reminders.Where(r => r.ExpiresAt <= now).Select(DispatchReminderAsync).ToArray();

        if (!dueReminders.Any())
            return;
        
        await Task.WhenAll(dueReminders);
    }

    /// <summary>
    ///     Removes a reminder.
    /// </summary>
    /// <param name="id">The ID of the reminder to remove.</param>
    public async Task<Result> RemoveReminderAsync(int id)
    {
        ReminderEntity? reminder = _reminders.SingleOrDefault(r => r.Id == id);
        if (reminder is null)
        {
            _logger.LogWarning(EventIds.Service, "Reminder was not present in memory. Was it dispatched already?");
        }
        else
        {
            _reminders.Remove(reminder);
            _logger.LogDebug("Removed reminder {Reminder}", id);
            
            SilkMetric.LoadedReminders.Dec();
        }
        
        return await _mediator.Send(new RemoveReminder.Request(id));
    }

    private async Task<Result> DispatchReminderAsync(ReminderEntity reminder)
    {
        _logger.LogDebug(EventIds.Service, "Dispatching expired reminder");
        
        await RemoveReminderAsync(reminder.Id);
        
        using (SilkMetric.ReminderDispatchTime.NewTimer())
        {
            if (reminder.IsPrivate)
                return await AttemptDispatchDMReminderAsync(reminder);
            
            return await AttemptDispatchReminderAsync(reminder);
        }
    }

    /// <summary>
    ///     Attempts to dispatch a reminder to a user in the same channel as the reminder, falling back to a DM if the channel is unavailable.
    /// </summary>
    /// <param name="reminder">The reminder to dispatch.</param>
    /// <returns>A result indicating whether the operation succeeded.</returns>
    private async Task<Result> AttemptDispatchReminderAsync(ReminderEntity reminder)
    {
        _logger.LogDebug("Attempting to dispatch reminder to guild channel {ChannelID}", reminder.ChannelID);
        
        var now = DateTimeOffset.UtcNow;
        var replyExists = false;

        if (reminder.ReplyMessageID is not null)
        {
            var reply = reminder.ReplyMessageID.Value;
            
            var replyResult = await _channels.GetChannelMessageAsync(reminder.ChannelID, reply);
            replyExists = replyResult.IsSuccess;
        }

        var reminderMessage = await _channels.GetChannelMessageAsync(reminder.ChannelID, reminder.MessageID.Value);

        var originalMessageExists = reminderMessage.IsSuccess;

        var dispatchMessage = GetReminderMessageString(reminder, replyExists, originalMessageExists).ToString();

        var dispatchResult = await _channels.CreateMessageAsync
        (
            reminder.ChannelID,
            dispatchMessage,
            // 1 << 12 is SupressNotifications
            flags: reminder.IsQuiet ? (MessageFlags) (1 << 12) : default,
            allowedMentions: 
            new AllowedMentions
            (
                Users: new[] { reminder.OwnerID },
                MentionRepliedUser: !reminder.IsReply
            ),
            messageReference: 
            new MessageReference
            (
                reminder.ReplyMessageID ?? reminder.MessageID ?? default,
                reminder.ChannelID,
                FailIfNotExists: false
            )
        ); 
        
        if (dispatchResult.IsSuccess) 
        {
            _logger.LogDebug(EventIds.Service, "Successfully dispatched reminder in {DispatchTime:N0} ms.", (DateTimeOffset.UtcNow - now).TotalMilliseconds);

            return Result.FromSuccess();
        }
        
        _logger.LogWarning(EventIds.Service, "Failed to dispatch reminder. Falling back to a DM.");
        _logger.LogError(EventIds.Service, dispatchResult.GetDeepestError()!.Message);

        var fallbackResult = await AttemptDispatchDMReminderAsync(reminder);

        if (fallbackResult.IsSuccess)
            return Result.FromSuccess();
        
        _logger.LogError(EventIds.Service, "Failed to dispatch reminder. Giving up.");

        return Result.FromError(fallbackResult.Error);
    }

    /// <summary>
    ///     Creates a formatted reminder message string.
    /// </summary>
    /// <param name="reminder">The reminder.</param>
    /// <param name="replyExists">Whether the reply (if any) still exists.</param>
    /// <param name="originalMessageExists">Whether the invocation message for the reminder still exists.</param>
    /// <returns>A StringBuilder containing the built message.</returns>
    private static StringBuilder GetReminderMessageString(ReminderEntity reminder, bool replyExists, bool originalMessageExists)
    {
        var dispatchMessage = new StringBuilder();

        if (reminder.IsPrivate)
        {
            dispatchMessage.AppendLine($"Hi! {reminder.CreatedAt.ToTimestamp()}, you asked me to remind you about this!");

            if (!string.IsNullOrWhiteSpace(reminder.MessageContent))
                dispatchMessage.AppendLine($"> {reminder.MessageContent}");            

            if (reminder.IsReply)
            {
                dispatchMessage.AppendLine($"You set a reminder on <@{reminder.ReplyAuthorID}>'s message:")
                               .AppendLine("> " + reminder.ReplyMessageContent.Truncate(800, "[...]").Replace("\n", "\n> "))
                               .AppendLine()
                               .AppendLine($"Which was posted here: https://discordapp.com/channels/{reminder.GuildID?.Value.ToString() ?? "@me"}/{reminder.ChannelID}/{reminder.ReplyMessageID}");
            }
        }
        else if (reminder.IsReply)
        {
            if (replyExists)
            {
                dispatchMessage.AppendLine($"Hey, <@{reminder.OwnerID}>, you set a reminder on this message {reminder.CreatedAt.ToTimestamp()}!");
            }
            else
            {
                dispatchMessage.AppendLine($"{reminder.CreatedAt.ToTimestamp()}, you asked me to remind ytou about a message, but it's disappeared!.")
                               .AppendLine("Here's what you replied to when you set the reminder, though!")
                               .AppendLine($"From <@{reminder.ReplyAuthorID}>:");

                if (string.IsNullOrWhiteSpace(reminder.ReplyMessageContent))
                    dispatchMessage.AppendLine("(No content.)");
                else
                    dispatchMessage.AppendLine("> " + reminder.ReplyMessageContent.Truncate(800, "[...]").Replace("\n", "\n> "));
            }

            if (!string.IsNullOrWhiteSpace(reminder.MessageContent))
            {
                dispatchMessage
                   .AppendLine("There was also additional context:")
                   .AppendLine("> " + reminder.MessageContent.Truncate(1800, "[...]").Replace("\n", "\n> "));
            }
        }
        else
        {
            if (originalMessageExists)
            {
                dispatchMessage.AppendLine($"Hi! {reminder.CreatedAt.ToTimestamp()}, you set a reminder.");
            }
            else
            {
                dispatchMessage.AppendLine($"Hi <@{reminder.OwnerID}! You set a reminder {reminder.CreatedAt.ToTimestamp()} but the message it referred to has disappeared!");
                dispatchMessage.AppendLine("I couldn't find the original message, but here's what you wanted to be reminded of:");
            }


            if (!string.IsNullOrWhiteSpace(reminder.MessageContent))
                dispatchMessage.AppendLine($"> {reminder.MessageContent.Truncate(1800, "[...]").Replace("\n", "\n> ")} \n\n");
        }
        return dispatchMessage;
    }

    /// <summary>
    ///     Dispatches a reminder to a user directly.
    /// </summary>
    /// <param name="reminder">The reminder to dispatch.</param>
    /// <returns>A result that may or may have not succeeded.</returns>
    private async Task<Result> AttemptDispatchDMReminderAsync(ReminderEntity reminder)
    {
        _logger.LogDebug(EventIds.Service, "Attempting to dispatch reminder to {OwnerID}.", reminder.OwnerID);
        
        var message = GetReminderMessageString(reminder, false, true).ToString();

        DateTimeOffset now = DateTimeOffset.UtcNow;

        Result<IChannel> channelRes = await _users.CreateDMAsync(reminder.OwnerID);

        if (!channelRes.IsSuccess)
        {
            _logger.LogError(EventIds.Service, "Failed to create a DM channel with {Owner}", reminder.OwnerID);

            return Result.FromError(channelRes.Error);
        }

        var flags = reminder.IsQuiet ? (MessageFlags) (1 << 12) : default;
        
        Result<IMessage> messageRes = await _channels.CreateMessageAsync(channelRes.Entity.ID, message, flags: flags);

        if (!messageRes.IsSuccess)
        {
            _logger.LogError(EventIds.Service, "Failed to dispatch reminder to {Owner}.", reminder.OwnerID);
            return Result.FromError(messageRes.Error);
        }
        
        _logger.LogDebug(EventIds.Service, "Successfully dispatched reminder in {ExecutionTime:N0} ms.", (DateTimeOffset.UtcNow - now).TotalMilliseconds);
        return Result.FromSuccess();
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        DateTime now = DateTime.UtcNow;
        _logger.LogInformation(EventIds.Service, "Loading reminders...");

        IEnumerable<ReminderEntity> reminders = await _mediator.Send(new GetAllReminders.Request(_shard.ShardCount, _shard.ShardID), cancellationToken);
        _reminders = reminders.ToList();

        _logger.LogInformation(EventIds.Service, "Loaded {ReminderCount} reminders in {ExecutionTime:N0} ms", _reminders.Count, (DateTime.UtcNow - now).TotalMilliseconds);
        
        SilkMetric.LoadedReminders.IncTo(_reminders.Count);
        
        _timer.Start();

        _logger.LogInformation(EventIds.Service, "Reminder service started.");
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer.Stop();
        _reminders.Clear();

        _logger.LogInformation(EventIds.Service, "Reminder service stopped.");

        return Task.CompletedTask;
    }
}