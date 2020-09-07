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

        public async Task Ping(CommandContext ctx)
        {
            Bot.CommandTimer.Stop();
            var embed = new DiscordEmbedBuilder()
                .WithAuthor(ctx.User.Username, iconUrl: ctx.User.AvatarUrl)
                .WithTitle("Pong! Silk! at the ready.")
                .WithColor(DiscordColor.Blue);
            var sw = new Stopwatch();
            sw.Start();
            var message = await ctx.RespondAsync(embed: embed);
            sw.Stop();
            embed.WithDescription($"***```cs\nBot Response Latency: {sw.ElapsedMilliseconds} ms. \n \nAPI Response Latency: {ctx.Client.Ping} ms.\n\nCommand Parsing Latency: {Bot.CommandTimer.ElapsedMilliseconds} ms.```***")
                .WithFooter("Silk!", ctx.Client.CurrentUser.AvatarUrl)
                .WithTimestamp(DateTime.Now);
            await message.ModifyAsync(embed: embed.Build());


        }

    }
}
