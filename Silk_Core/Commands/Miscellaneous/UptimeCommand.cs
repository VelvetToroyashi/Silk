using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Humanizer;
using Humanizer.Localisation;
using SilkBot.Utilities;

namespace SilkBot.Commands.Miscellaneous
{
    [Category(Categories.Misc)]
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class UptimeCommand : BaseCommandModule
    {
        [Command]
        public async Task UpTime(CommandContext ctx)
        {
            var now = DateTime.Now;
            var uptime = now.Subtract(SilkBot.Bot.StartupTime);
            await ctx.RespondAsync($"Running for `{uptime.Humanize(4, null, TimeUnit.Month, TimeUnit.Second)}.`");
        }

    }
}
