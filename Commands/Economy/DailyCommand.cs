using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using SilkBot.Economy;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SilkBot.Commands.Economic_Commands
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
                EconomicUsers.Instance.Add(ctx, ctx.Member);
                var embed = EconomicUsers.Instance.Users[ctx.User.Id].DoDaily(ctx);
                await ctx.RespondAsync(embed: embed);
            }
        }
    }
}
