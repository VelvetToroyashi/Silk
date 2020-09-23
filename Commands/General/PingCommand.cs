using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data.Common;
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
            var sw = Stopwatch.StartNew();
            var message = await ctx.RespondAsync(embed: embed);
            sw.Stop();
            embed.WithDescription(
                $"***```cs\nBot Response Latency: {sw.ElapsedMilliseconds} ms.  \n\n" +
                $"API Response Latency: {ctx.Client.Ping} ms.\n\n" +
                $"Message Processing Latency: {SilkBot.Bot.CommandTimer.ElapsedTicks / 10} µs.\n\n" +
                $"Database latency: {GetDbLatency(ctx.Guild.Id)} ms.```***")
                .WithFooter("Silk!", ctx.Client.CurrentUser.AvatarUrl)
                .WithTimestamp(DateTime.Now);
            await message.ModifyAsync(embed: new Optional<DiscordEmbed>(embed));


        }

        private int GetDbLatency(ulong guildId)
        {
            var sw = Stopwatch.StartNew();
            var db = new SilkDbContext();
            //_ = db.Guilds.First(_ => _.DiscordGuildId == guildId);
            db.Database.BeginTransaction();
            db.Database.ExecuteSqlRaw("SELECT * FROM Guilds;");
            
            sw.Stop();
            return (int)sw.ElapsedMilliseconds;
        }

    }
}
