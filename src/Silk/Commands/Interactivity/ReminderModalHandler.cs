using System;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Interactivity;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Extensions;
using Silk.Services.Bot;
using Silk.Services.Guild;

namespace Silk.Commands.Interactivity;

public class ReminderModalHandler : InteractionGroup
{
    private const string ReminderTimeNotPresent = "It seems you didn't specify a time in your reminder.\n" +
                                                  "I can recognize times like 10m, 5h, 2h30m, and even natural language like 'three hours from now' and 'in 2 days'";

    private readonly TimeHelper                 _timeHelper;
    private readonly ReminderService            _reminders;
    private readonly InteractionContext         _context;
    private readonly IDiscordRestChannelAPI     _channels;
    private readonly IDiscordRestInteractionAPI _interactions;
    
    public ReminderModalHandler
    (
        TimeHelper timeHelper,
        ReminderService reminders,
        InteractionContext context,
        IDiscordRestChannelAPI channels,
        IDiscordRestInteractionAPI interactions
    )
    {
        _timeHelper   = timeHelper;
        _reminders    = reminders;
        _context      = context;
        _channels     = channels;
        _interactions = interactions;
    }
    
    [Modal("reminder-modal")]
    public async Task<Result> HandleInteractionAsync(Snowflake reply, string when, string? what = null)
    {
        var offset     = await _timeHelper.GetOffsetForUserAsync(_context.User.ID);
        var timeResult = _timeHelper.ExtractTime(when, offset, out _);

        if (!timeResult.IsDefined(out var parsedTime))
        {
            var informResult = await _interactions.CreateFollowupMessageAsync
            (
             _context.ApplicationID,
             _context.Token,
             timeResult.Error!.Message,
             flags: MessageFlags.Ephemeral,
             ct: this.CancellationToken
            );
            
           return (Result)informResult;
        }

        if (parsedTime <= TimeSpan.Zero)
        {
            var informResult = await _interactions.CreateFollowupMessageAsync
            (
             _context.ApplicationID,
             _context.Token,
             "It seems you specified a time in the past.\n" +
             "Please specify a time in the future.",
             flags: MessageFlags.Ephemeral,
             ct: this.CancellationToken
            );
            
           return (Result)informResult;
        }

        if (parsedTime < TimeSpan.FromMinutes(3))
        {
            var minTimeResult = await _interactions.CreateFollowupMessageAsync
            (
             _context.ApplicationID,
             _context.Token,
             "You can't set a reminder less than three minutes!",
             flags: MessageFlags.Ephemeral,
             ct: this.CancellationToken
            );
            
           return (Result)minTimeResult;
        }
        
        var reminderTime = DateTimeOffset.UtcNow + parsedTime;

        var messageResult = await _channels.GetChannelMessageAsync(_context.ChannelID, reply, this.CancellationToken);
        
        if (!messageResult.IsDefined(out var message))
            return Result.FromError(messageResult.Error!);
        
        await _reminders.CreateReminderAsync
        (
         reminderTime,
         _context.User.ID,
         _context.ChannelID,
         null,
         null,
         what,
         message.Content,
         reply,
         message.Author.ID
        );

        var res =  await _interactions.CreateFollowupMessageAsync
        (
         _context.ApplicationID,
         _context.Token,
         $"Done! I'll remind you {reminderTime.ToTimestamp()}!",
         flags: MessageFlags.Ephemeral,
         ct: this.CancellationToken
        );

        return (Result)res;
    }
}