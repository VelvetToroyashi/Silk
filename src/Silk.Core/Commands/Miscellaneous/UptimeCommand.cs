using System;
using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Humanizer;
using Humanizer.Localisation;
using Silk.Core.Services.Bot;
using Silk.Core.Utilities.HelpFormatter;

namespace Silk.Core.Commands.Miscellaneous
{
    [Category(Categories.Misc)]
    public class UptimeCommand : BaseCommandModule
    {
        private readonly UptimeService _uptime;
        public UptimeCommand(UptimeService uptime)
        {
            _uptime = uptime;
        }

        [Command]
        [Description("See how long Silk has been running!")]
        public async Task UpTime(CommandContext ctx)
        {
            TimeSpan uptime = _uptime.UpTime - DateTime.Now;
            await ctx.RespondAsync($"I've been running for `{uptime.Humanize(3, CultureInfo.InvariantCulture, TimeUnit.Month, TimeUnit.Second)}`");
            if (_uptime.LastOutage != DateTime.MinValue)
            {
                TimeSpan lastOutage = _uptime.LastOutage - DateTime.Now;
                await ctx.RespondAsync($"Last outage recorded: {_uptime.LastOutage} ({lastOutage.Humanize(2, CultureInfo.InvariantCulture, TimeUnit.Week, TimeUnit.Second)} ago). Outage lasted {_uptime.OutageTime.Humanize(2, CultureInfo.InvariantCulture, TimeUnit.Day, TimeUnit.Second)}.");
            }
        }
    }
}