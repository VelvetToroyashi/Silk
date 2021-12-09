using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Results;
using Silk.Core.Data.Entities;
using Silk.Core.Data.MediatR.Reminders;
using Silk.Core.Types;
using Silk.Shared.Constants;

namespace Silk.Core.Services.Server;

public sealed class ReminderService : IHostedService
{
    private readonly ILogger<ReminderService> _logger;
       
    private readonly IMediator                _mediator;
        
    private readonly IDiscordRestUserAPI    _userApi;
    private readonly IDiscordRestChannelAPI _channelApi;
        
    private List<ReminderEntity> _reminders = new(); // We're gonna slurp all reminders into memory. Yolo, I guess.
    private readonly AsyncTimer           _timer;
        
    public ReminderService(ILogger<ReminderService> logger, IMediator mediator, IDiscordRestUserAPI userApi, IDiscordRestChannelAPI channelApi)
    {
        _logger = logger;
        _mediator = mediator;
        _userApi = userApi;
        _channelApi = channelApi;

        _timer = new(TryDispatchRemindersAsync, TimeSpan.FromSeconds(1), true);
    }
        
    public async Task CreateReminder
    (
        DateTime expiry,
        ulong    owner,
        ulong    channel,                         
        ulong    message, 
        ulong?   guild,
        string?  conent,                    
        bool     isReply      = false,
        ulong?   reply        = null,
        ulong?   replyAuthor  = null,
        string?  replyContent = null
    )
    {
        ReminderEntity reminder = await _mediator.Send(new CreateReminderRequest(expiry, owner, channel, message, guild, conent, isReply, reply, replyAuthor, replyContent));
        _reminders.Add(reminder);
    }

    /// <summary>
    /// Gets all reminders that are due for a certain user.
    /// </summary>
    /// <param name="userId">The ID of the user to search reminders for.</param>
    /// <returns>The specified user's reminders.</returns>
    public IEnumerable<ReminderEntity> GetRemindersAsync(ulong userId) => _reminders.Where(r => r.OwnerId == userId);

    /// <summary>
    /// The main dispatch loop, which iterates all active reminders, and dispatches them if they're due.
    /// </summary>
    private async Task TryDispatchRemindersAsync()
    {
        if (!_reminders.Any())
            return;
            
        var now = DateTime.UtcNow;
        var reminders = _reminders.Where(r => r.Expiration <= now);

        await Task.WhenAll(reminders.Select(DispatchReminderAsync));
    }
        
    /// <summary>
    /// Removes a reminder.
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

        if (reminder.MessageId is 0)
            return AttemptDispatchDMReminderAsync(reminder);
            
        return AttemptDispatchReminderAsync(reminder);
    }
        
    /// <summary>
    /// Attempts to dispatch a reminder to a user in the same channel as the reminder, falling back to a DM if the channel is unavailable.
    /// </summary>
    /// <param name="reminder">The reminder to dispatch.</param>
    /// <returns>A result indicating whether the opreation succeeded.</returns>
    private async Task<Result> AttemptDispatchReminderAsync(ReminderEntity reminder)
    {
        var now = DateTimeOffset.UtcNow;
        var replyExists = false;

        if (reminder.ReplyId is ulong reply)
        {
            var replyResult = await _channelApi.GetChannelMessageAsync(new(reminder.ChannelId), new(reply));
            replyExists = replyResult.IsSuccess;
        }
            
        var reminderMessage = await _channelApi.GetChannelMessageAsync(new(reminder.ChannelId), new(reminder.MessageId));

        var originalMessageExists = reminderMessage.IsSuccess;

        var dispatchMessage = GetReminderMessageString(reminder, false, replyExists, originalMessageExists).ToString();

        var dispatchReuslt = await _channelApi.CreateMessageAsync(new(reminder.ChannelId), dispatchMessage);

        if (dispatchReuslt.IsSuccess)
        {
            _logger.LogDebug(EventIds.Service, "Successfully dispatched reminder in {DispatchTime} ms.", (DateTimeOffset.UtcNow - now).TotalMilliseconds.ToString("N1"));
               
            await RemoveReminderAsync(reminder.Id);

            return Result.FromSuccess();
        }
        else
        {
            _logger.LogWarning(EventIds.Service, "Failed to dispatch reminder. Falling back to a DM.");
                
            var fallbackResult = await AttemptDispatchDMReminderAsync(reminder);

            if (fallbackResult.IsSuccess)
            {
                _logger.LogDebug(EventIds.Service, "Successfully dispatched reminder in {DispatchTime} ms.", (DateTimeOffset.UtcNow - now).TotalMilliseconds.ToString("N1"));
                    
                return Result.FromSuccess();
            }
            else
            {
                _logger.LogError(EventIds.Service, "Failed to dispatch reminder. Giving up.");
                    
                return Result.FromError(fallbackResult.Error);
            }
        }
    }
        
    /// <summary>
    /// Creates a formatted reminder message string.
    /// </summary>
    /// <param name="reminder">The reminder.</param>
    /// <param name="inDMs">Whether this reminder is being sent in DMs.</param>
    /// <param name="replyExists">Whether the reply (if any) still exists.</param>
    /// <param name="originalMessageExists">Whether the invocation messsage for the reminder still exists.</param>
    /// <returns>A StringBuilder contianing the built message.</returns>
    private static StringBuilder GetReminderMessageString(ReminderEntity reminder, bool inDMs, bool replyExists, bool originalMessageExists)
    {
        var dispatchMessage = new StringBuilder();
            
        bool isReply = reminder.ReplyId is ulong;

        if (inDMs)
        {
            dispatchMessage.AppendLine("Hey! You asked me to remind you about this:");
            dispatchMessage.AppendLine(reminder.MessageContent);
        }
        else if (isReply)
        {
            dispatchMessage.AppendLine($"Hey, <@{reminder.OwnerId}>! You asked me to remind you about this!");
                
            if (!replyExists)
                dispatchMessage.AppendLine("I couldn't find the message I was supposed to reply to.")
                    .AppendLine("Here's what you replied to, when you set the reminder, though!")
                    .AppendLine($"From <@{reminder.ReplyAuthorId}>:")
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
            
        dispatchMessage.AppendLine($"This reminder was set <t:{((DateTimeOffset)reminder.CreationTime).ToUnixTimeSeconds()}:R> ago!");
        return dispatchMessage;
    }

    /// <summary>
    /// Dispatches a reminder to a user directly.
    /// </summary>
    /// <param name="reminder">The reminder to dispatch.</param>
    /// <returns>A result that may or may have not succeeded.</returns>
    private async Task<Result> AttemptDispatchDMReminderAsync(ReminderEntity reminder)
    {
        await RemoveReminderAsync(reminder.Id);

        var message = GetReminderMessageString(reminder, true, false, true).ToString();
            
        var now = DateTimeOffset.UtcNow;
            
        var channelRes = await _userApi.CreateDMAsync(new(reminder.OwnerId));

        if (!channelRes.IsSuccess)
        {
            _logger.LogError(EventIds.Service,"Failed to create a DM channel with {Owner}", reminder.OwnerId);

            return Result.FromError(channelRes.Error);
        }
            
        var messageRes = await _channelApi.CreateMessageAsync(channelRes.Entity.ID, message);
            
        if (!messageRes.IsSuccess)
        {
            _logger.LogError(EventIds.Service,"Failed to dispatch reminder to {Owner}.", reminder.OwnerId);
            return Result.FromError(messageRes.Error);
        }
        else
        {
            _logger.LogDebug(EventIds.Service, "Successfully dispatched reminder in {ExecutionTime} ms", (now - DateTimeOffset.UtcNow).TotalMilliseconds);
            return Result.FromSuccess();  
        }
    }
        
    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        _logger.LogInformation(EventIds.Service, "Loading reminders...");

        var reminders = await _mediator.Send(new GetAllRemindersRequest());
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