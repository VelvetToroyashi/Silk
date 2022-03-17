using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Data.Entities;
using Silk.Data.MediatR.Reminders;
using Silk.Extensions;
using Silk.Shared.Constants;
using Silk.Shared.Types;

namespace Silk.Services.Guild;

public sealed class ReminderService : IHostedService
{
    private readonly ILogger<ReminderService> _logger;

    private readonly IMediator _mediator;

    private readonly IDiscordRestUserAPI    _userApi;
    private readonly IDiscordRestChannelAPI _channelApi;

    // When it comes to sharding, ideally this is only the reminders for the guilds that are in the shard.
    // Perhaps we'll filter manually with `.Where(r => r.GuildID >> 22 % ShardCount == ShardId)`
    private          List<ReminderEntity> _reminders = new(); 
    private readonly AsyncTimer           _timer;

    public ReminderService(ILogger<ReminderService> logger, IMediator mediator, IDiscordRestUserAPI userApi, IDiscordRestChannelAPI channelApi)
    {
        _logger     = logger;
        _mediator   = mediator;
        _userApi    = userApi;
        _channelApi = channelApi;

        _timer = new(TryDispatchRemindersAsync, TimeSpan.FromSeconds(1), true);
    }

    public async Task CreateReminderAsync
        (
            DateTimeOffset   expiry,
            Snowflake  ownerID,
            Snowflake  channelID,
            Snowflake?  messageID,
            Snowflake? guildID,
            string?    content,
            string?    replyContent = null,
            Snowflake? replyID       = null,
            Snowflake? replyAuthorID = null
        )
    {
        ReminderEntity reminder = await _mediator.Send(new CreateReminder.Request(expiry, ownerID, channelID, messageID, guildID, content, replyID, replyAuthorID, replyContent));
        _reminders.Add(reminder);
    }

    /// <summary>
    ///     Gets all reminders that are due for a certain user.
    /// </summary>
    /// <param name="userID">The ID of the user to search reminders for.</param>
    /// <returns>The specified user's reminders.</returns>
    public IEnumerable<ReminderEntity> GetReminders(Snowflake userID) => _reminders.Where(r => r.OwnerID == userID);

    /// <summary>
    ///     The main dispatch loop, which iterates all active reminders, and dispatches them if they're due.
    /// </summary>
    private async Task TryDispatchRemindersAsync()
    {
        if (!_reminders.Any())
            return;

        DateTime                    now       = DateTime.UtcNow;
        IEnumerable<ReminderEntity> reminders = _reminders.Where(r => r.ExpiresAt <= now);

        await Task.WhenAll(reminders.ToList().Select(DispatchReminderAsync));
    }

    /// <summary>
    ///     Removes a reminder.
    /// </summary>
    /// <param name="id">The ID of the reminder to remove.</param>
    public async Task RemoveReminderAsync(int id)
    {
        ReminderEntity? reminder = _reminders.SingleOrDefault(r => r.Id == id);
        if (reminder is null)
        {
            _logger.LogWarning(EventIds.Service, "Reminder was not present in memory. Was it dispatched already?");
        }
        else
        {
            _reminders.Remove(reminder);
            await _mediator.Send(new RemoveReminder.Request(id));
        }
    }

    private Task<Result> DispatchReminderAsync(ReminderEntity reminder)
    {
        _logger.LogDebug(EventIds.Service, "Dispatching reminder");

        if (reminder.MessageID is null)
            return AttemptDispatchDMReminderAsync(reminder);

        return AttemptDispatchReminderAsync(reminder);
    }

    /// <summary>
    ///     Attempts to dispatch a reminder to a user in the same channel as the reminder, falling back to a DM if the channel is unavailable.
    /// </summary>
    /// <param name="reminder">The reminder to dispatch.</param>
    /// <returns>A result indicating whether the operation succeeded.</returns>
    private async Task<Result> AttemptDispatchReminderAsync(ReminderEntity reminder)
    {
        var now       = DateTimeOffset.UtcNow;
        var replyExists = false;

        if (reminder.ReplyMessageID is not null)
        {
            var reply = reminder.ReplyMessageID.Value;
            
            var replyResult = await _channelApi.GetChannelMessageAsync(reminder.ChannelID, reply);
            replyExists = replyResult.IsSuccess;
        }

        var reminderMessage = await _channelApi.GetChannelMessageAsync(reminder.ChannelID, reminder.MessageID.Value);

        var originalMessageExists = reminderMessage.IsSuccess;

        var dispatchMessage = GetReminderMessageString(reminder, replyExists, originalMessageExists).ToString();

        var dispatchReuslt = await _channelApi.CreateMessageAsync
            (
             reminder.ChannelID,
             dispatchMessage,
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
        
        if (dispatchReuslt.IsSuccess)
        {
            _logger.LogDebug(EventIds.Service, "Successfully dispatched reminder in {DispatchTime:N0} ms.", (DateTimeOffset.UtcNow - now).TotalMilliseconds);

            await RemoveReminderAsync(reminder.Id);

            return Result.FromSuccess();
        }
        
        _logger.LogWarning(EventIds.Service, "Failed to dispatch reminder. Falling back to a DM.");

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
    /// <param name="inDMs">Whether this reminder is being sent in DMs.</param>
    /// <param name="replyExists">Whether the reply (if any) still exists.</param>
    /// <param name="originalMessageExists">Whether the invocation message for the reminder still exists.</param>
    /// <returns>A StringBuilder containing the built message.</returns>
    private static StringBuilder GetReminderMessageString(ReminderEntity reminder, bool replyExists, bool originalMessageExists)
    {
        var dispatchMessage = new StringBuilder();
        
        if (reminder.IsPrivate)
        {
            dispatchMessage.AppendLine("Hey! You asked me to remind you about this:");
            dispatchMessage.AppendLine(reminder.MessageContent);
        }
        else if (reminder.IsReply)
        {
            dispatchMessage.AppendLine($"Hey, <@{reminder.OwnerID}>! You asked me to remind you about this!");

            if (!replyExists)
                dispatchMessage.AppendLine("I couldn't find the message I was supposed to reply to.")
                               .AppendLine("Here's what you replied to, when you set the reminder, though!")
                               .AppendLine($"From <@{reminder.ReplyAuthorID}>:")
                               .AppendLine("> " + reminder.ReplyMessageContent);

            if (!string.IsNullOrEmpty(reminder.MessageContent))
                dispatchMessage.AppendLine("There was also additional context:")
                               .AppendLine($"> {reminder.MessageContent}");
        }
        else
        {
            dispatchMessage.AppendLine("Hey, you wanted to be reminded of this!");

            if (!originalMessageExists)
                dispatchMessage.AppendLine("I couldn't find the original message, but here's what you wanted to be reminded of:");

            dispatchMessage.AppendLine($"> {reminder.MessageContent} \n\n");
        }

        dispatchMessage.AppendLine($"This reminder was set {reminder.CreatedAt.ToTimestamp()}!");
        return dispatchMessage;
    }

    /// <summary>
    ///     Dispatches a reminder to a user directly.
    /// </summary>
    /// <param name="reminder">The reminder to dispatch.</param>
    /// <returns>A result that may or may have not succeeded.</returns>
    private async Task<Result> AttemptDispatchDMReminderAsync(ReminderEntity reminder)
    {
        await RemoveReminderAsync(reminder.Id);

        var message = GetReminderMessageString(reminder, false, true).ToString();

        DateTimeOffset now = DateTimeOffset.UtcNow;

        Result<IChannel> channelRes = await _userApi.CreateDMAsync(reminder.OwnerID);

        if (!channelRes.IsSuccess)
        {
            _logger.LogError(EventIds.Service, "Failed to create a DM channel with {Owner}", reminder.OwnerID);

            return Result.FromError(channelRes.Error);
        }

        Result<IMessage> messageRes = await _channelApi.CreateMessageAsync(channelRes.Entity.ID, message);

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

        IEnumerable<ReminderEntity> reminders = await _mediator.Send(new GetAllReminders.Request(), cancellationToken);
        _reminders = reminders.ToList();

        _logger.LogInformation(EventIds.Service, "Loaded {ReminderCount} reminders in {ExecutionTime:N0} ms", _reminders.Count, (DateTime.UtcNow - now).TotalMilliseconds);

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