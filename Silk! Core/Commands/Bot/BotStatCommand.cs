using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Humanizer;
using Humanizer.Localisation;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SilkBot.Commands.Bot
{
    public class BotStatCommand : BaseCommandModule
    {
        [Command("Stats"), Aliases("botstats", "botinfo")]
        public async Task BotStat(CommandContext ctx)
        {
            var process = Process.GetCurrentProcess();
            using (var cpu = new PerformanceCounter("Process", "% Processor Time", "_Total"))
            {
                int guildCount = ctx.Client.Guilds.Count;

                var embed = new DiscordEmbedBuilder()
                    .WithTitle($"Stats for Silk!:")
                    .WithColor(DiscordColor.Gold)
                    .AddField("Latency", $"{ctx.Client.Ping}ms", true)
                    .AddField("Total guilds", $"{guildCount}", true)
                    .AddField("Shards", $"{ctx.Client.ShardCount}", true)
                    .AddField("Memory", $"{process.PrivateMemorySize64 / 1024 / 1024.0:n2} MB", true)
                    .AddField("CPU:", $"~{cpu.NextValue() / Environment.ProcessorCount:n2}%", true)
                    .AddField("Threads", $"{process.Threads.Count}", true)
                    .AddField("Uptime", (DateTime.Now - process.StartTime).Humanize(3, minUnit: TimeUnit.Second), false);
                await ctx.RespondAsync(embed: embed);
                cpu.Dispose();
            }
        }
    }
}
