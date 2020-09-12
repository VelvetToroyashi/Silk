using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;


namespace SilkBot.Commands.General
{
    public class PingCommand : BaseCommandModule
    {
        [Command("Ping")]

        public async Task Ping(CommandContext ctx)
        {
            SilkBot.Bot.CommandTimer.Stop();
            var embed = new DiscordEmbedBuilder()
                .WithAuthor(ctx.User.Username, iconUrl: ctx.User.AvatarUrl)
                .WithTitle("Pong! Silk! at the ready.")
                .WithColor(DiscordColor.Blue);
            var sw = new Stopwatch();
            sw.Start();
            var message = await ctx.RespondAsync(embed: embed);
            sw.Stop();
            embed.WithDescription($"***```cs\nBot Response Latency: {sw.ElapsedMilliseconds} ms. \n \nAPI Response Latency: {ctx.Client.Ping} ms.\n\nMessage Processing Latency: {SilkBot.Bot.CommandTimer.ElapsedMilliseconds} ms.\n\n" +
                $"Database latency: {GetDbLatency(ctx.Guild.Id)} ms.```***")
                .WithFooter("Silk!", ctx.Client.CurrentUser.AvatarUrl)
                .WithTimestamp(DateTime.Now);
            await message.ModifyAsync(embed: embed.Build());


        }

        private int GetDbLatency(ulong guildId)
        {
            var sw = Stopwatch.StartNew();
            _ = SilkBot.Bot.Instance.SilkDBContext.Guilds.First(_ => _.DiscordGuildId == guildId);
            
            sw.Stop();
            return (int)sw.ElapsedTicks / 1000;
        }

    }
}
