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
using Silk.Core.Data.Entities;
using Silk.Core.Data.MediatR.Reminders;
using Silk.Core.Types;
using Silk.Shared.Constants;

namespace Silk.Core.Services.Server
{
    public sealed class ReminderService : IHostedService
    {
        private readonly ILogger<ReminderService> _logger;
       
        private readonly IMediator                _mediator;
        
        private readonly IDiscordRestUserAPI    _userApi;
        private readonly IDiscordRestChannelAPI _channelApi;
        
        private List<ReminderEntity> _reminders; // We're gonna slurp all reminders into memory. Yolo, I guess.
        private AsyncTimer           _timer;

        private CancellationTokenSource _cts = new();
        
        public ReminderService(ILogger<ReminderService> logger, IMediator mediator, IDiscordRestUserAPI userApi, IDiscordRestChannelAPI channelApi)
        {
            _logger = logger;
            _mediator = mediator;
            _userApi = userApi;
            _channelApi = channelApi;

            _timer = new(DispatchLoopAsync, TimeSpan.FromSeconds(1));
        }
        
        public async Task CreateReminder
        (
            DateTime     expiry,
            ulong        owner,
            ulong        channel,                         
            ulong        message, 
            ulong?       guild,
            string?      conent,                    
            bool         isReply      = false,
            ReminderType type         = ReminderType.Once,
            ulong?       reply        = null,
            ulong?       replyAuthor  = null,
            string?      replyContent = null
        )
        {
            ReminderEntity reminder = await _mediator.Send(new CreateReminderRequest(expiry, owner, channel, message, guild, conent, isReply, type, reply, replyAuthor, replyContent));
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
        private async Task DispatchLoopAsync()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                if (_reminders.Count is 0)
                {
                    await Task.Delay(400);
                    continue;
                }
                
                var now = DateTime.UtcNow;
                var reminders = _reminders.Where(r => r.Expiration <= now);

                await Task.WhenAll(reminders.Select(DispatchReminderAsync));
            }
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
            
            string? replyLink = null;

            if (reminder.ReplyId is ulong reply)
            {
                var replyResult = await _channelApi.GetChannelMessageAsync(new(reminder.ChannelId), new(reply));
                
                if (replyResult.IsSuccess)
                {
                    var replyMessage = replyResult.Entity;
                    replyLink = $"https://discordapp.com/channels/{replyMessage.GuildID}/{replyMessage.ChannelID}/{replyMessage.ID}";
                }
            }
            
            var reminderMessage = await _channelApi.GetChannelMessageAsync(new(reminder.ChannelId), new(reminder.MessageId));

            var originalMessageExists = reminderMessage.IsSuccess;
            
            var dispatchMessage = GetReminderMessage
                    (
                     reminder.CreationTime,
                     reminder.MessageContent!,
                     reminder.OwnerId,
                     originalMessageExists,
                     !originalMessageExists,
                     !originalMessageExists ? null : "I couldn't seem to find your oringinal message.",
                     replyLink,
                     reminder.ReplyMessageContent,
                     reminder.ReplyAuthorId
                    );
            
            var dispatchReuslt =  await _channelApi.CreateMessageAsync
                (
                 new(reminder.ChannelId),
                 dispatchMessage,
                 messageReference: reminder.ReplyId is ulong rpid && replyLink is not null 
                     ? new  MessageReference(new Snowflake(rpid), new Snowflake(reminder.ChannelId))
                     : default(Optional<IMessageReference>));

            if (dispatchReuslt.IsSuccess)
            {
                _logger.LogDebug(EventIds.Service, "Successfully dispatched reminder in {DispatchTime} ms.", (now - DateTimeOffset.UtcNow).TotalMilliseconds.ToString("N1"));
               
                await RemoveReminderAsync(reminder.Id);

                return Result.FromSuccess();
            }
            else
            {
                _logger.LogWarning(EventIds.Service, "Failed to dispatch reminder. Falling back to a DM.");
                
                var fallbackResult = await AttemptDispatchDMReminderAsync(reminder);

                if (fallbackResult.IsSuccess)
                {
                    _logger.LogDebug(EventIds.Service, "Successfully dispatched reminder in {DispatchTime} ms.", (now - DateTimeOffset.UtcNow).TotalMilliseconds.ToString("N1"));
                    
                    return Result.FromSuccess();
                }
                else
                {
                    _logger.LogError(EventIds.Service, "Failed to dispatch reminder. Giving up.");

                    await RemoveReminderAsync(reminder.Id);
                    
                    return Result.FromError(fallbackResult.Error);
                }
            }
        }
        
        /// <summary>
        /// Dispatches a reminder to a user directly.
        /// </summary>
        /// <param name="reminder">The reminder to dispatch.</param>
        /// <returns>A result that may or may have not succeeded.</returns>
        private async Task<Result> AttemptDispatchDMReminderAsync(ReminderEntity reminder)
        {
            var message = GetReminderMessage(reminder.CreationTime, reminder.MessageContent!, reminder.OwnerId);
            
            var now = DateTimeOffset.UtcNow;
            
            var channelRes = await _userApi.CreateDMAsync(new(reminder.OwnerId));

            if (!channelRes.IsSuccess)
            {
                _logger.LogError(EventIds.Database,"Failed to create a DM channel with {Owner}", reminder.OwnerId);
                return Result.FromError(channelRes.Error);
            }
            
            var messageRes = await _channelApi.CreateMessageAsync(channelRes.Entity.ID, reminder.MessageContent!);

            await RemoveReminderAsync(reminder.Id);
            
            if (!messageRes.IsSuccess)
            {
                _logger.LogError(EventIds.Database,"Failed to dispatch reminder to {Owner}.", reminder.OwnerId);
                return Result.FromError(messageRes.Error);
            }
            else
            {
                _logger.LogDebug(EventIds.Service, "Successfully dispatched reminder in {ExecutionTime} ms", (now - DateTimeOffset.UtcNow).TotalMilliseconds);
                return Result.FromSuccess();  
            }
        }
        
        /// <summary>
        /// Gets a formatted reminder message.
        /// </summary>
        /// <param name="creationTime">The time the reminder was created.</param>
        /// <param name="reminderMessage">The content of the reminder.</param>
        /// <param name="ownerID">The owner of the reminder.</param>
        /// <param name="isReplying">Whether this reminder is replying directly to the invocation message.</param>
        /// <param name="mention">Whether the author of the reminder should be mentioned in the message.</param>
        /// <param name="errorMessage">If there was an issue, this will be appended to the reminder.</param>
        /// <param name="replyLink">If the invocation message of the reminder is a reply, the message link of the reply.</param>
        /// <param name="replyText">If the invocation message of the reminder is a reply, the content of the reply.</param>
        /// <param name="replyAuthorID">If the invocation message of the reminder is a reply, the author of the reply, to mention.</param>
        /// <returns>The formatted reply message.</returns>
        private string GetReminderMessage
        (
            DateTimeOffset creationTime,
            string         reminderMessage,
            ulong          ownerID,
            bool           isReplying    = false,
            bool           mention       = false,
            string?        errorMessage  = null,
            string?        replyLink     = null,
            string?        replyText     = null,
            ulong?         replyAuthorID = null
        )
        {
            var message = new StringBuilder();
            
            if (mention)
                message.AppendLine($"Hey, <@{ownerID}>! You wanted to be reminded of something, but {errorMessage ?? "(There was no error message specified...That's not good!)"}");
            else if (isReplying)
                message.AppendLine("Hey! You wanted to be reminded of this!");
            else 
                message.AppendLine("Hey, you wanted to be reminded of something! I've come to remind you of that:");

            if (!isReplying)
            {
                message.AppendLine(reminderMessage);

                if (replyText is not null)
                {
                    message.AppendLine($"Replying to <@{replyAuthorID}>:")
                           .AppendLine($"> {replyText}")
                           .AppendLine(replyLink);
                }
                
                message.AppendLine($"This reminder was set <t:{creationTime.ToUnixTimeSeconds()}:r>");
            }
            
            return message.ToString();
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
            _cts.Cancel();
            _timer.Stop();
            _reminders.Clear();
            
            _logger.LogInformation(EventIds.Service, "Reminder service stopped.");
            
            return Task.CompletedTask;
        }
    }
}