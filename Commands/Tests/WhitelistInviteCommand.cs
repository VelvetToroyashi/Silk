using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SilkBot.Commands.Tests
{
    public class WhitelistInviteCommand : BaseCommandModule
    {
        [Command("Whitelist")]
        public async Task WhitelistInvite(CommandContext ctx, [RemainingText]params string[] links)
        {
            if(links.Any(l => l.StartsWith("remove")))
            {
                var removeIndex = links.ToList().IndexOf("remove") + 1;
                if (removeIndex > links.Length) await ctx.RespondAsync("You must supply a link to remove!");
                return;
            }
            var config = SilkBot.Bot.Instance.SilkDBContext.Guilds.First(g => g.DiscordGuildId == ctx.Guild.Id);
            if (!config.WhiteListInvites)
            {
                await ctx.RespondAsync("This server doesn't whitelist invites!");
                return;
            }
            if (links.Length == 0)
            {
                await ctx.RespondAsync($"Whitelisted links: {string.Join(", ", config.WhiteListedLinks.Select(_ => $"`{_.Link}`"))}");
                return;
            }
            
            foreach(var invite in links)
            {
                var formattedInvite = string.Empty;
                formattedInvite = invite.Contains('/') ? invite.Replace("discord.com/invite", "discord.gg/") : "discord.gg/" + invite;
                config.WhiteListedLinks.Add(new Models.WhiteListedLink { Link = formattedInvite });
            }
            await SilkBot.Bot.Instance.SilkDBContext.SaveChangesAsync();
            await ctx.RespondAsync($"Whitelisted {links.Length} invites!");
        }
    }
}
