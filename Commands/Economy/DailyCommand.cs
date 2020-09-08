using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using SilkBot.Economy;

namespace SilkBot.Commands.Economy
{
    public class DailyCommand : BaseCommandModule
    {
        [Command("Daily")]
        public async Task DailyMoney(CommandContext ctx)
        {
            if (EconomicUsers.Instance.UserExists(ctx.User.Id))
            {
                var embed = EconomicUsers.Instance.Users[ctx.User.Id].DoDaily(ctx);
                await ctx.RespondAsync(embed: embed);
            }
            else
            {
                EconomicUsers.Instance.Add(ctx.Member);
                var embed = EconomicUsers.Instance.Users[ctx.User.Id].DoDaily(ctx);
                await ctx.RespondAsync(embed: embed);
            }
        }
    }
}
