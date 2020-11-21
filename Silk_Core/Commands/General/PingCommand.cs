using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using SilkBot.Utilities;


namespace SilkBot.Commands.General
{
    [Category(Categories.General)]
    public class PingCommand : BaseCommandModule
    {
        private readonly IDbContextFactory<SilkDbContext> _dbFactory;
        public PingCommand(IDbContextFactory<SilkDbContext> dbFactory) => _dbFactory = dbFactory;

        [Command("Ping")]
        public async Task Ping(CommandContext ctx)
        {
            SilkBot.Bot.CommandTimer.Stop();
            var embed = new DiscordEmbedBuilder()
                .WithAuthor(ctx.User.Username, iconUrl: ctx.User.AvatarUrl)
                .WithTitle("Ping? Sure!")
                .WithColor(DiscordColor.Blue);
            var sw = Stopwatch.StartNew();
            var message = await ctx.RespondAsync(embed: embed);
            sw.Stop();
            await Task.Delay(200);
            embed.WithDescription(
                $"***```cs\nBot Response Latency: {sw.ElapsedMilliseconds} ms.\n\n" +
                $"API Response Latency: {ctx.Client.Ping} ms.\n\n" +
                $"Processing Latency: {SilkBot.Bot.CommandTimer.ElapsedTicks / 10} µs.\n\n" +
                $"Database latency: {GetDbLatency()} ms.```***")
                .WithFooter("Silk!", ctx.Client.CurrentUser.AvatarUrl)
                .WithTimestamp(DateTime.Now);
            await message.ModifyAsync(embed: embed.Build());
        }

        private int GetDbLatency()
        {
            var sw = Stopwatch.StartNew();
            using var db = _dbFactory.CreateDbContext();
            //_ = db.Guilds.First(_ => _.DiscordGuildId == guildId);
            db.Database.BeginTransaction();
            db.Database.ExecuteSqlRaw("SELECT first_value(\"Id\") over () FROM \"Guilds\"");

            sw.Stop();
            return (int)sw.ElapsedMilliseconds;
        }

    }
}
