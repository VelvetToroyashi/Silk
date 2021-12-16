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
using Remora.Rest.Core;
using Remora.Results;
using Silk.Data.Entities;
using Silk.Data.MediatR.Reminders;
using Silk.Shared.Constants;
using Silk.Shared.Types;

namespace Silk.Services.Guild;

public sealed class ReminderService : IHostedService
{
    private readonly ILogger<ReminderService> _logger;

    private readonly IMediator _mediator;

    private readonly IDiscordRestUserAPI    _userApi;
    private readonly IDiscordRestChannelAPI _channelApi;

    private          List<ReminderEntity> _reminders = new(); // We're gonna slurp all reminders into memory. Yolo, I guess.
    private readonly AsyncTimer           _timer;

    public ReminderService(ILogger<ReminderService> logger, IMediator mediator, IDiscordRestUserAPI userApi, IDiscordRestChannelAPI channelApi)
    {
        _logger     = logger;
        _mediator   = mediator;
        _userApi    = userApi;
        _channelApi = channelApi;

        _timer = new(TryDispatchRemindersAsync, TimeSpan.FromSeconds(1), true);
    }

    public async Task CreateReminder
        (
            DateTime   expiry,
            Snowflake  ownerID,
            Snowflake  channelID,
            Snowflake  messageID,
            Snowflake? guildID,
            string?    content,
            string?    replyContent = null,
            Snowflake? replyID       = null,
            Snowflake? replyAuthorID = null
        )
    {
        ReminderEntity reminder = await _mediator.Send(new CreateReminderRequest(expiry, ownerID, channelID, messageID, guildID, content, replyID, replyAuthorID, replyContent));
        _reminders.Add(reminder);
    }

    /// <summary>
    ///     Gets all reminders that are due for a certain user.
    /// </summary>
    /// <param name="userID">The ID of the user to search reminders for.</param>
    /// <returns>The specified user's reminders.</returns>
    public IEnumerable<ReminderEntity> GetRemindersAsync(Snowflake userID) => _reminders.Where(r => r.OwnerID == userID);

    /// <summary>
    ///     The main dispatch loop, which iterates all active reminders, and dispatches them if they're due.
    /// </summary>
    private async Task TryDispatchRemindersAsync()
    {
        if (!_reminders.Any())
            return;

        DateTime                    now       = DateTime.UtcNow;
        IEnumerable<ReminderEntity> reminders = _reminders.Where(r => r.ExpiresAt <= now);

        await Task.WhenAll(reminders.Select(DispatchReminderAsync));
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
            await _mediator.Send(new RemoveReminderRequest(id));
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
    /// <returns>A result indicating whether the opreation succeeded.</returns>
    private async Task<Result> AttemptDispatchReminderAsync(ReminderEntity reminder)
    {
        DateTimeOffset now         = DateTimeOffset.UtcNow;
        var            replyExists = false;

        if (reminder.ReplyID is not null)
        {
            var reply = reminder.ReplyID.Value;
            
            Result<IMessage> replyResult = await _channelApi.GetChannelMessageAsync(reminder.ChannelID, reply);
            replyExists = replyResult.IsSuccess;
        }

        Result<IMessage> reminderMessage = await _channelApi.GetChannelMessageAsync(reminder.ChannelID, reminder.MessageID.Value);

        bool originalMessageExists = reminderMessage.IsSuccess;

        var dispatchMessage = GetReminderMessageString(reminder, false, replyExists, originalMessageExists).ToString();

        Result<IMessage> dispatchReuslt = await _channelApi.CreateMessageAsync(reminder.ChannelID, dispatchMessage);

        if (dispatchReuslt.IsSuccess)
        {
            _logger.LogDebug(EventIds.Service, "Successfully dispatched reminder in {DispatchTime} ms.", (DateTimeOffset.UtcNow - now).TotalMilliseconds.ToString("N1"));

            await RemoveReminderAsync(reminder.Id);

            return Result.FromSuccess();
        }
        _logger.LogWarning(EventIds.Service, "Failed to dispatch reminder. Falling back to a DM.");

        Result fallbackResult = await AttemptDispatchDMReminderAsync(reminder);

        if (fallbackResult.IsSuccess)
        {
            _logger.LogDebug(EventIds.Service, "Successfully dispatched reminder in {DispatchTime} ms.", (DateTimeOffset.UtcNow - now).TotalMilliseconds.ToString("N1"));

            return Result.FromSuccess();
        }
        _logger.LogError(EventIds.Service, "Failed to dispatch reminder. Giving up.");

        return Result.FromError(fallbackResult.Error);
    }

    /// <summary>
    ///     Creates a formatted reminder message string.
    /// </summary>
    /// <param name="reminder">The reminder.</param>
    /// <param name="inDMs">Whether this reminder is being sent in DMs.</param>
    /// <param name="replyExists">Whether the reply (if any) still exists.</param>
    /// <param name="originalMessageExists">Whether the invocation messsage for the reminder still exists.</param>
    /// <returns>A StringBuilder contianing the built message.</returns>
    private static StringBuilder GetReminderMessageString(ReminderEntity reminder, bool inDMs, bool replyExists, bool originalMessageExists)
    {
        var dispatchMessage = new StringBuilder();

        bool isReply = reminder.ReplyID is not null;

        if (inDMs)
        {
            dispatchMessage.AppendLine("Hey! You asked me to remind you about this:");
            dispatchMessage.AppendLine(reminder.MessageContent);
        }
        else if (isReply)
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
            dispatchMessage.AppendLine("Hey, you wanted to be reminded of this.");

            if (!originalMessageExists)
                dispatchMessage.AppendLine("I couldn't find the original message, but here's what you wanted to be reminded of:");

            dispatchMessage.AppendLine(reminder.MessageContent);
        }

        dispatchMessage.AppendLine($"This reminder was set <t:{((DateTimeOffset)reminder.CreatedAt).ToUnixTimeSeconds()}:R> ago!");
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

        var message = GetReminderMessageString(reminder, true, false, true).ToString();

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
        _logger.LogDebug(EventIds.Service, "Successfully dispatched reminder in {ExecutionTime} ms", (now - DateTimeOffset.UtcNow).TotalMilliseconds);
        return Result.FromSuccess();
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        DateTime now = DateTime.UtcNow;
        _logger.LogInformation(EventIds.Service, "Loading reminders...");

        IEnumerable<ReminderEntity> reminders = await _mediator.Send(new GetAllRemindersRequest());
        _reminders = reminders.ToList();

        _logger.LogInformation(EventIds.Service, "Loaded {ReminderCount} reminders in {ExecutionTime} ms", _reminders.Count, (DateTime.UtcNow - now).TotalMilliseconds);

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