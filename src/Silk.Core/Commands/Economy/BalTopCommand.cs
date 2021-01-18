using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Database;
using Silk.Core.Database.Models;
using Silk.Core.Utilities;

namespace Silk.Core.Commands.Economy
{
    [Category(Categories.Economy)]
    public class BalTopCommand : BaseCommandModule
    {
        private readonly IDbContextFactory<SilkDbContext> _dbFactory;

        public BalTopCommand(IDbContextFactory<SilkDbContext> dbContextFactory)
        {
            _dbFactory = dbContextFactory;
        }

        [RequireGuild]
        [Command("baltop")]
        [Aliases("highroller")]
        [Description("See which members have the most money!")]
        public async Task BalTop(CommandContext ctx)
        {
            SilkDbContext db = _dbFactory.CreateDbContext();
            List<GlobalUserModel> economyUsers = db.GlobalUsers
                .OrderByDescending(user => user.Cash)
                .Take(10)
                .ToList();

            if (!economyUsers.Any())
            {
                await ctx.RespondAsync("Seems you don't have any members with economy accounts. " +
                                       $"Ask some members to use `{ctx.Prefix}daily` and I'll set up accounts for them *:)*");
                return;
            }

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithTitle("Members with the Highest Economy Balances! :money_with_wings:")
                .WithColor(DiscordColor.Gold);

            for (var i = 0; i < economyUsers.Count; ++i)
            {
                var position = i + 1;
                var economyUser = economyUsers[i];
                var displayName = ctx.Guild.Members[economyUser.Id].DisplayName;

                embed.AddField(i == 0 ? $"{position}. :crown:" : $"{position}. ", $"{displayName}", true);
            }

            await ctx.RespondAsync(embed: embed);
        }
    }
}