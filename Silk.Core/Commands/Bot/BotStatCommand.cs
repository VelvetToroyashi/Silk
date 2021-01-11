#pragma warning disable CA1822 // Mark members as static

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Humanizer;
using Humanizer.Localisation;
using Silk.Core.Utilities;

#pragma warning disable 1591

namespace Silk.Core.Commands.Bot
{
    [Category(Categories.Bot)]
    public class BotStatCommand : BaseCommandModule
    {
        [Command("botstats")]
        [Aliases("botinfo")]
        public async Task BotStat(CommandContext ctx)
        {
            using var process = Process.GetCurrentProcess();
            int guildCount = ctx.Client.Guilds.Count;

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithTitle("Stats for Silk!:")
                .WithColor(DiscordColor.Gold)
                .AddField("Latency", $"{ctx.Client.Ping}ms", true)
                .AddField("Total guilds", $"{guildCount}", true)
                .AddField("Shards", $"{ctx.Client.ShardCount}", true)
                .AddField("Memory", $"{process.PrivateMemorySize64 / 1024 / 1024:n2} MB", true)
                .AddField("Threads", $"{process.Threads.Count}", true)
                .AddField("Uptime",
                    (DateTime.Now - process.StartTime).Humanize(3, minUnit: TimeUnit.Second));
            await ctx.RespondAsync(embed: embed);
        }
    }
}