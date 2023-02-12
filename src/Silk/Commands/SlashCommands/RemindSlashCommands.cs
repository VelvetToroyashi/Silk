using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humanizer;
using Humanizer.Localisation;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Interactivity;
using Remora.Results;
using Silk.Extensions;
using Silk.Extensions.Remora;
using Silk.Services.Bot;
using Silk.Services.Guild;
using Silk.Utilities;

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
    [CommandType(ApplicationCommandType.User)]
    public async Task<IResult> RemindAsync(IUser user)
    {
        var components = new IMessageComponent[]
        {
            new ActionRowComponent(new[]
            {
                //TODO: Improve via caching instead of this
                new TextInputComponent("reply", TextInputStyle.Short, "Reply ID (do not modify)", 15, 20, true, _context.Interaction.Data.Value.AsT0.Resolved.Value.Messages.Value.Values.First().ID.Value.ToString(), "What did I say? >:C") 
            }),
            new ActionRowComponent(new[]
            {
                new TextInputComponent("when", TextInputStyle.Short, "When?", 2, 100, true, "in 10 minutes", "\"10m\", \"in three days\", etc.")
            }),
            new ActionRowComponent(new[]
            {
                new TextInputComponent("what", TextInputStyle.Paragraph, "Additional context?", default, 1500, false, default, "Add pineapples out of spite.")
            })
        };

        var data = new InteractionModalCallbackData(CustomIDHelpers.CreateModalID("reminder-modal"), "Set a reminder!", components);

        return await _interactions.CreateInteractionResponseAsync(_context.Interaction.ID, _context.Interaction.Token, new InteractionResponse(InteractionCallbackType.Modal, new(data)));
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
        [Description("When should I remind you? (e.g. 5m, in 5 minutes, tonight at 5 (requires timezone), etc.)")]
        string rawTime,
        
        [Option("about")]
        [Description("What should I remind you about?")]
        string about,
        
        [Option("silent")]
        [Description("Should this reminder be delivered quietly?")]
        bool silent = false
    )
    {
        var offset     = await _timeHelper.GetOffsetForUserAsync(_context.GetUserID());
        var timeResult = _timeHelper.ExtractTime(rawTime, offset, out _);

        if (!timeResult.IsDefined(out var parsedTime))
            return await _interactions.EditOriginalInteractionResponseAsync
            (
             _context.Interaction.ApplicationID,
             _context.Interaction.Token,
             timeResult.Error!.Message
            );
        
        if (parsedTime <= TimeSpan.Zero)
            return await _interactions.EditOriginalInteractionResponseAsync
            (
             _context.Interaction.ApplicationID,
             _context.Interaction.Token,
            "You can't set a reminder in the past!"
            );
        
        if (parsedTime < _minimumReminderTime)
            return await _interactions.EditOriginalInteractionResponseAsync
            (
             _context.Interaction.ApplicationID,
             _context.Interaction.Token,
             $"You can't set a reminder less than {_minimumReminderTime.Humanize(minUnit: TimeUnit.Minute)}!"
            );
    
        var reminderTime = DateTimeOffset.UtcNow + parsedTime;
        
        await _reminders.CreateReminderAsync
        (
         reminderTime,
         _context.GetUserID(),
         _context.GetChannelID(),
         null,
         _context.Interaction.GuildID.IsDefined(out var guild) ? guild : null,
         about,
         isSilent: silent
        );

        return await _interactions.EditOriginalInteractionResponseAsync
        (
             _context.Interaction.ApplicationID,
             _context.Interaction.Token,
             $"Done! I'll remind you {reminderTime.ToTimestamp()}!"
        );
    }

    [Command("list")]
    [Description("List all your reminders!")]
    public async Task<IResult> ListRemindersAsync()
    {
        var reminders = (await _reminders.GetUserRemindersAsync(_context.GetUserID())).OrderBy(r => r.ExpiresAt);
        
        if (!reminders.Any())
            return await _interactions.EditOriginalInteractionResponseAsync
                (
                 _context.Interaction.ApplicationID,
                 _context.Interaction.Token,
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
         _context.Interaction.ApplicationID,
         _context.Interaction.Token,
         embeds: new[] { embed }
        );
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
            return await _interactions.EditOriginalInteractionResponseAsync
            (
             _context.Interaction.ApplicationID,
             _context.Interaction.Token,
             "You don't have any active reminders!"
            );

        var reminder = reminders.FirstOrDefault(r => r.Id == reminderID);

        if (reminder is null)
            return await _interactions.EditOriginalInteractionResponseAsync
            (
             _context.Interaction.ApplicationID,
             _context.Interaction.Token,
             "You don't have a reminder by that ID!"
            );

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

        return await _interactions.EditOriginalInteractionResponseAsync
        (
         _context.Interaction.ApplicationID,
         _context.Interaction.Token,
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
        var reminders = await _reminders.GetUserRemindersAsync(_context.GetUserID());
        
        if (reminders.All(r => r.Id != reminderID))
            return await _interactions.EditOriginalInteractionResponseAsync
            (
             _context.Interaction.ApplicationID,
             _context.Interaction.Token,
             "You don't have any reminders, or at least not one by that ID!"
            );

        await _reminders.RemoveReminderAsync(reminderID);
        
        return await _interactions.EditOriginalInteractionResponseAsync
        (
         _context.Interaction.ApplicationID,
         _context.Interaction.Token,
         "I've cancelled your reminder!"
        );
    }
}