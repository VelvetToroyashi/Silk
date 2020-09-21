
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace SilkBot.Commands.Server
{
    public class ServerInfoCommand : BaseCommandModule
    {
        [Command]
        public async Task ServerInfo(CommandContext ctx)
        {
            var guild = ctx.Guild;
            var embed = new DiscordEmbedBuilder().WithColor(DiscordColor.Gold).WithFooter($"Silk! | Requested by: {ctx.User.Id}");
            embed.AddField("Boosts:", guild.PremiumSubscriptionCount.Value.ToString());
            embed.AddField("Verification Level:", guild.VerificationLevel.ToString());
            await ctx.RespondAsync(embed: embed);
        }
    }
}
