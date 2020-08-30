using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SilkBot
{
   
    [Description("Contact the owner or join support server!")]
    public class Owner : BaseCommandModule
    {
        [Command("Owner")]
        [HelpDescription("Bot bugged? Contact the owner.")]
        public async Task Contact(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder()
                .WithAuthor(name: ctx.Member.DisplayName, iconUrl: ctx.Member.AvatarUrl)
                .WithColor(DiscordColor.CornflowerBlue)
                .WithDescription($"Need help? You can contact the owner: <@209279906280898562>")
                .WithFooter("Silk")
                .WithTimestamp(DateTime.Now);
            await ctx.RespondAsync(null, false, embed);
        }

        [Command("Support")]
        [HelpDescription("Want more immediate help? Join the support server!")]
        public async Task Support(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder()
                .WithAuthor(ctx.Member.DisplayName, null, ctx.Member.AvatarUrl)
                .WithColor(DiscordColor.CornflowerBlue)
                .WithDescription($"Need help? Join the [Silk! Support server](https://discord.gg/HZfZb95) **←←**")
                .WithFooter("Silk")
                .WithTimestamp(DateTime.Now);
            await ctx.RespondAsync(null, false, embed);
        }
    }
}
