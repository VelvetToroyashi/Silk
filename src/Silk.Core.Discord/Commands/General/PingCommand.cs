using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data;
using Silk.Core.Discord.Utilities.HelpFormatter;

namespace Silk.Core.Discord.Commands.General
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
                .AddField("→ Discord API latency ←", "```cs\n" + $"{ctx.Client.Ping} ms".PadLeft(10, '⠀') + "```", true)
                .AddField("→ Silk! API Latency ←", "```cs\n" + "Calculating..".PadLeft(15, '⠀') + "```", true)
                // Make the databse latency centered. //
                .AddField("​", "​", true)
                .AddField("→ Database Latency ←", "```cs\n" + "Calculating..".PadLeft(15, '⠀') + "```", true)
                .AddField("​", "​", true);



            var sw = Stopwatch.StartNew();
            DiscordMessage message = await ctx.RespondAsync(embed);
            sw.Stop();

            await Task.Delay(400);
            var silkApiResponse = await new Ping().SendPingAsync("velvetthepanda.dev", 50);
            embed
                .ClearFields()
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