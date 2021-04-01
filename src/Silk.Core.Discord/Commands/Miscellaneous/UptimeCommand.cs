using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Humanizer;
using Humanizer.Localisation;
using Silk.Core.Discord.Utilities.HelpFormatter;

namespace Silk.Core.Discord.Commands.Miscellaneous
{
    [Category(Categories.Misc)]
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class UptimeCommand : BaseCommandModule
    {
        [Command]
        [Description("See how long Silk has been running!")]
        public async Task UpTime(CommandContext ctx)
        {
            DateTime now = DateTime.Now;
            TimeSpan uptime = now.Subtract(Program.Startup);
            await ctx.RespondAsync($"Running for `{uptime.Humanize(4, null, TimeUnit.Month, TimeUnit.Second)}.`");
        }
    }
}