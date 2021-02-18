using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Utilities.HelpFormatter;
using Silk.Data;

namespace Silk.Core.Commands.General
{
    [Category(Categories.General)]
    public class PingCommand : BaseCommandModule
    {
        private readonly IDbContextFactory<SilkDbContext> _dbFactory;

        public PingCommand(IDbContextFactory<SilkDbContext> dbFactory) => _dbFactory = dbFactory;

        [Command("ping")]
        [Aliases("pong")]
        [Description("Check the responsiveness of Silk")]
        public async Task Ping(CommandContext ctx)
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder().WithColor(DiscordColor.Blue);

            var sw = Stopwatch.StartNew();
            DiscordMessage message = await ctx.RespondAsync(embed);
            sw.Stop();

            await Task.Delay(100);
            var silkApiResponse = await new Ping().SendPingAsync("velvetthepanda.dev", 50);
            embed
                .AddField("→ Message Latency ←", "```cs\n" + $"{sw.ElapsedMilliseconds} ms".PadLeft(10, '⠀') + "```", true)
                .AddField("→ Discord API latency ←", "```cs\n" + $"{ctx.Client.Ping} ms".PadLeft(10, '⠀') + "```", true)
                .AddField("→ Silk! API Latency ←", "```cs\n" + $"{silkApiResponse.RoundtripTime} ms".PadLeft(10, '⠀') + "```", true)
                // Make the databse latency centered. //
                .AddField("​", "​", true)
                .AddField("→ Database Latency ←", "```cs\n" + $"{GetDbLatency()} ms".PadLeft(10, '⠀') + "```", true)
                .AddField("​", "​", true)
                .WithFooter($"Silk! | Requested by {ctx.User.Id}", ctx.User.AvatarUrl);

            await message.ModifyAsync(embed.Build());
        }

        private int GetDbLatency()
        {
            SilkDbContext db = _dbFactory.CreateDbContext();
            //_ = db.Guilds.First(_ => _.DiscordGuildId == guildId);
            db.Database.BeginTransaction();
            var sw = Stopwatch.StartNew();
            db.Database.ExecuteSqlRaw("SELECT first_value(\"Id\") OVER () FROM \"Guilds\"");
            sw.Stop();
            return (int) sw.ElapsedMilliseconds;
        }
    }
}