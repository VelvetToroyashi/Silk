using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SilkBot
{
    public class PingCommand : BaseCommandModule
    {
        [Command("Ping")]
        [HelpDescription("Returns my API response time.")]
        public async Task Ping(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder()
                .WithAuthor(ctx.Member.DisplayName, iconUrl: ctx.Member.AvatarUrl)
                .WithTitle("Pong! Silk! at the ready.")
                .WithColor(DiscordColor.Blue);
            var sw = new Stopwatch();
            sw.Start();
            var message = await ctx.RespondAsync(embed: embed);
            sw.Stop();
            embed.WithDescription($"***```cs\nClient Latency: {sw.ElapsedMilliseconds} ms. \n \nGateway latency: {ctx.Client.Ping} ms.```***")
                .WithFooter("Silk!", ctx.Client.CurrentUser.AvatarUrl)
                .WithTimestamp(DateTime.Now);
            await message.ModifyAsync(embed: embed.Build());
            

        }

    }
}
