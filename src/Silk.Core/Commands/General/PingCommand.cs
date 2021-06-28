using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data;
using Silk.Core.Utilities.HelpFormatter;

namespace Silk.Core.Commands.General
{
    [Category(Categories.Misc)]
    public class PingCommand : BaseCommandModule
    {
        private readonly IDbContextFactory<GuildContext> _dbFactory;

        public PingCommand(IDbContextFactory<GuildContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        [Command("ping")]
        [Aliases("pong")]
        [Description("Check the responsiveness of Silk")]
        public async Task Ping(CommandContext ctx)
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Blue)
                .AddField("→ Message Latency ←", "```cs\n" + "Calculating..".PadLeft(15, '⠀') + "```", true)
                .AddField("→ Websocket latency ←", "```cs\n" + $"{ctx.Client.Ping} ms".PadLeft(10, '⠀') + "```", true)
                .AddField("→ Silk! API Latency ←", "```cs\n" + "Calculating..".PadLeft(15, '⠀') + "```", true)
                // Make the databse latency centered. //
                .AddField("→ Database Latency ←", "```cs\n" + "Calculating..".PadLeft(15, '⠀') + "```", true)
                .AddField("​", "​", true)
                .AddField("→ Discord API Latency ←", "```cs\n" + "Calculating..".PadLeft(15, '⠀') + "```", true);

            DiscordMessage message = await ctx.RespondAsync(embed);


            var now = DateTime.UtcNow;
            await ctx.Channel.TriggerTypingAsync();

            var apiLat = (DateTime.UtcNow - now).TotalMilliseconds.ToString("N0");
            await Task.Delay(200);
            PingReply silkApiResponse = await new Ping().SendPingAsync("velvetthepanda.dev", 50);
            embed
                .ClearFields()
                .AddField("→ Message Latency ←", "```cs\n" + $"{(message.CreationTimestamp - ctx.Message.CreationTimestamp).Milliseconds} ms".PadLeft(10, '⠀') + "```", true)
                .AddField("→ Websocket latency ←", "```cs\n" + $"{ctx.Client.Ping} ms".PadLeft(10, '⠀') + "```", true)
                .AddField("→ Silk! API Latency ←", "```cs\n" + $"{silkApiResponse.RoundtripTime} ms".PadLeft(10, '⠀') + "```", true)
                // Make the databse latency centered. //
                .AddField("→ Database Latency ←", "```cs\n" + $"{GetDbLatency()} ms".PadLeft(10, '⠀') + "```", true)
                .AddField("​", "​", true)
                .AddField("→ Discord API Latency ←", "```cs\n" + $"{apiLat} ms".PadLeft(12) + "```", true)
                .WithFooter($"Silk! | Requested by {ctx.User.Id}", ctx.User.AvatarUrl);

            await message.ModifyAsync(embed.Build());
        }

        private int GetDbLatency()
        {
            GuildContext db = _dbFactory.CreateDbContext();
            //_ = db.Guilds.First(_ => _.DiscordGuildId == guildId);
            db.Database.BeginTransaction();
            var sw = Stopwatch.StartNew();
            db.Database.ExecuteSqlRaw("SELECT first_value(\"Id\") OVER () FROM \"Guilds\"");
            sw.Stop();
            return (int) sw.ElapsedMilliseconds;
        }
    }
}