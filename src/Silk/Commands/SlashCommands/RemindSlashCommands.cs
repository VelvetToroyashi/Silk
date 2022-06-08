using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using Humanizer.Localisation;
using Recognizers.Text.DateTime.Wrapper;
using Recognizers.Text.DateTime.Wrapper.Models.BclDateTime;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Results;
using Silk.Commands.General;
using Silk.Extensions;
using Silk.Extensions.Remora;
using Silk.Services.Bot;
using Silk.Services.Guild;

namespace Silk.Commands.SlashCommands;

[SlashCommand]
public class RemindContextCommands : CommandGroup
{
    private readonly InteractionContext         _context;
    private readonly IDiscordRestInteractionAPI _interactions;
    
    public RemindContextCommands(InteractionContext context, IDiscordRestInteractionAPI interactions)
    {
        _context      = context;
        _interactions = interactions;
    }

    [Command("Remind Me!")]
    [SuppressInteractionResponse(true)]
    [CommandType(ApplicationCommandType.Message)]
    public async Task<IResult> RemindAsync()
    {
        var components = new IMessageComponent[]
        {
            new ActionRowComponent(new[]
            {
                new TextInputComponent("time", TextInputStyle.Short, "When?", 2, 100, true, "10 minutes from now", "\"10m\", \"in three days\", etc.")
            }),
            new ActionRowComponent(new[]
            {
                new TextInputComponent(_context.Data.Resolved.Value.Messages.Value.Values.First().ID.Value.ToString(), TextInputStyle.Paragraph, "Additional context?", default, 1500, false, default, "Add pineapples out of spite.")
            })
        };

        var data = new InteractionModalCallbackData("reminder-modal", "Set a reminder!", components);

        return await _interactions.CreateInteractionResponseAsync(_context.ID, _context.Token, new InteractionResponse(InteractionCallbackType.Modal, new(data)));
    }
}

[Ephemeral]
[SlashCommand]
[Group("remind")]
public class RemindSlashCommands : CommandGroup
{
    private const string ReminderTimeNotPresent = "It seems you didn't specify a time in your reminder.\n" +
                                                  "I can recognize times like 10m, 5h, 2h30m, and even natural language like 'three hours from now' and 'in 2 days'";
    
    private static readonly TimeSpan _minimumReminderTime = TimeSpan.FromMinutes(3);

    private readonly TimeHelper                 _timeHelper;
    private readonly ReminderService            _reminders;
    private readonly InteractionContext         _context;
    private readonly IDiscordRestInteractionAPI _interactions;
    
    public RemindSlashCommands
    (
        TimeHelper                 timeHelper,
        ReminderService            reminders,
        InteractionContext         context,
        IDiscordRestInteractionAPI interactions
    )
    {
        _timeHelper      = timeHelper;
        _reminders       = reminders;
        _context         = context;
        _interactions    = interactions;
    }

    [Command("set")]
    [Description("Set a reminder!")]
    public async Task<IResult> SetReminderAsync
    (
        [Option("time")]
        [Description("When should I remind you? (e.g. in 5 minutes, in an hour, in 2 days, etc.)")]
        string rawTime,
        
        [Option("about")]
        [Description("What should I remind you about?")]
        string about
    )
    {
        var offset     = await _timeHelper.GetOffsetForUserAsync(_context.User.ID);
        var timeResult = _timeHelper.ExtractTime(rawTime, offset);

        if (!timeResult.IsDefined(out var parsedTime))
            return await _interactions.EditOriginalInteractionResponseAsync
            (
             _context.ApplicationID,
             _context.Token,
             timeResult.Error!.Message
            );
        
        if (parsedTime <= TimeSpan.Zero)
            return await _interactions.EditOriginalInteractionResponseAsync
            (
             _context.ApplicationID,
             _context.Token,
            "You can't set a reminder in the past!"
            );
        
        if (parsedTime < _minimumReminderTime)
            return await _interactions.EditOriginalInteractionResponseAsync
            (
             _context.ApplicationID,
             _context.Token,
             $"You can't set a reminder less than {_minimumReminderTime.Humanize(minUnit: TimeUnit.Minute)}!"
            );
    
        var reminderTime = DateTimeOffset.UtcNow + parsedTime;
        
        await _reminders.CreateReminderAsync
        (
         reminderTime,
         _context.User.ID,
         _context.ChannelID,
         null,
         _context.GuildID.IsDefined(out var guild) ? guild : null,
         about
        );

        return await _interactions.EditOriginalInteractionResponseAsync
            (
             _context.ApplicationID,
             _context.Token,
             $"Done! I'll remind you {reminderTime.ToTimestamp()}!"
            );
    }

    [Command("list")]
    [Description("List all your reminders!")]
    public async Task<IResult> ListRemindersAsync()
    {
        var reminders = (await _reminders.GetUserRemindersAsync(_context.User.ID)).OrderBy(r => r.ExpiresAt);
        
        if (!reminders.Any())
            return await _interactions.EditOriginalInteractionResponseAsync
                (
                 _context.ApplicationID,
                 _context.Token,
                 "You don't have any reminders!"
                );
        
        var formattedReminders = reminders.Select(r => $"`{r.Id}` expiring {r.ExpiresAt.ToTimestamp()}:\n" +
                                                       $"{r.MessageContent.Truncate(50, "[...]")}"         +
                                                       (r.IsReply ? $"\nReplying to [message](https://discordapp.com/channels/{r.GuildID}/{r.ChannelID}/{r.ReplyMessageID})" : ""));

        var embed = new Embed
        {
            Title       = "Your Reminders:",
            Colour      = Color.DodgerBlue,
            Description = formattedReminders.Join("\n\n")
        };
        
        return await _interactions.EditOriginalInteractionResponseAsync
            (
             _context.ApplicationID,
             _context.Token,
             embeds: new[] { embed }
            );
    }
    
    [Command("remove")]
    [Description("Remove a reminder!")]
    public async Task<IResult> RemoveReminderAsync
    (
        [Option("reminder")]
        [Description("The ID of the reminder to remove!")]
        int reminderID
    )
    {
        var reminders = await _reminders.GetUserRemindersAsync(_context.User.ID);
        
        if (reminders.All(r => r.Id != reminderID))
            return await _interactions.EditOriginalInteractionResponseAsync
                (
                 _context.ApplicationID,
                 _context.Token,
                 "You don't have any reminders, or at least not one by that ID!"
                );

        await _reminders.RemoveReminderAsync(reminderID);
        
        return await _interactions.EditOriginalInteractionResponseAsync
            (
             _context.ApplicationID,
             _context.Token,
             "I've cancelled your reminder!"
            );
    }
}