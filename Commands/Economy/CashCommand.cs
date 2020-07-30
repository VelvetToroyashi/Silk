using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SilkBot.Economy;
using System.Threading.Tasks;

namespace SilkBot.Commands.Economic_Commands
{
    public class CashCommand : BaseCommandModule
    {
        [Command("Cash")]
        [Aliases("Money", "mons", "mon", "monni", "moni", "Coins", "Tokens")]
        public async Task Cash(CommandContext ctx)
        {
            var userCoins = EconomicUsers.Instance.Users.TryGetValue(ctx.User.Id, out var user) ? user.Cash : 0;

            var eb = EmbedMaker.CreateEmbed(ctx, "Account balance:", $"You have {userCoins} coins!");
            var betterEmbed = new DiscordEmbedBuilder(eb).WithAuthor(name: ctx.User.Username, iconUrl: ctx.User.AvatarUrl);
            await ctx.RespondAsync(embed: betterEmbed);
        }
    }
}
