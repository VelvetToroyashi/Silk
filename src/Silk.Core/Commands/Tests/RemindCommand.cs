using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Humanizer;
using Humanizer.Localisation;
using Silk.Core.Services;
using Silk.Extensions;

namespace Silk.Core.Commands.Tests
{
    public class RemindCommand : BaseCommandModule
    {
        private readonly ReminderService _reminders;
        public RemindCommand(ReminderService reminders)
        {
            _reminders = reminders;
        }
        
        
        [Command]
        public async Task Remind(CommandContext ctx, TimeSpan time, [RemainingText] string reminder)
        {
            ulong? replyId = ctx.Message.ReferencedMessage?.Id;
            ulong? authorId = ctx.Message.ReferencedMessage?.Author?.Id;
            string? replyContent = ctx.Message.ReferencedMessage?.Content;
            
            await _reminders.CreateReminder(DateTime.Now + time, ctx.User.Id, ctx.Channel.Id,
                ctx.Message.Id, ctx.Guild.Id, reminder, ctx.Message.ReferencedMessage is not null, replyId, authorId, replyContent);
            await ctx.RespondAsync($"Alrighty, I'll remind you of {reminder.Pull(..1000)} in {time.Humanize(2, minUnit: TimeUnit.Second)}");
        }
    }
}