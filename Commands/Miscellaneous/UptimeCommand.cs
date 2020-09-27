using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Threading.Tasks;

namespace SilkBot.Commands.Miscellaneous
{
    public class UptimeCommand : BaseCommandModule
    {
        [Command]
        public async Task UpTime(CommandContext ctx)
        {
            var now = DateTime.Now;
            var uptime = now.Subtract(SilkBot.Bot.StartupTime);
            await ctx.RespondAsync($"Running for `{uptime.Days} days, {uptime.Hours} hours, {uptime.Minutes} minutes, and {uptime.Seconds} seconds.`");
        }

    }
}
