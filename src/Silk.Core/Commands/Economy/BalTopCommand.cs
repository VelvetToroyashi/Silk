using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Database;
using Silk.Core.Database.Models;
using Silk.Core.Utilities.HelpFormatter;

namespace Silk.Core.Commands.Economy
{
    [Category(Categories.Economy)]
    public class BalTopCommand : BaseCommandModule
    {
        private readonly IDbContextFactory<SilkDbContext> _dbFactory;

        public BalTopCommand(IDbContextFactory<SilkDbContext> dbContextFactory) => _dbFactory = dbContextFactory;

        [RequireGuild]
        [Command("top")]
        [Aliases("topbal")]
        [Description("See which members have the most money!")]
        public async Task BalTop(CommandContext ctx)
        {
            SilkDbContext db = _dbFactory.CreateDbContext();
            List<GlobalUser> economyUsers = db.GlobalUsers
                .OrderByDescending(user => user.Cash)
                .Take(10)
                .ToList();

            if (!economyUsers.Any())
            {
                await ctx.RespondAsync("Seems we don't have any members with accounts. " +
                                       $"Ask some members to use `{ctx.Prefix}daily` and I'll set up accounts for them *:)*");
                return;
            }

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithTitle("Members with the biggest stacks! :money_with_wings:")
                .WithColor(DiscordColor.Gold);
            var position = 1;

            if (economyUsers.Count > 3)
            {
                position = 4;
                GlobalUser? firstGlobalUser = economyUsers.First();
                DiscordUser? firstUser = await ctx.Client.GetUserAsync(firstGlobalUser.Id);

                GlobalUser? secondGlobalUser = economyUsers.Skip(1).First();
                DiscordUser? secondUser = await ctx.Client.GetUserAsync(secondGlobalUser.Id);

                GlobalUser? thirdGlobalUser = economyUsers.Skip(2).First();
                DiscordUser? thirdUser = await ctx.Client.GetUserAsync(thirdGlobalUser.Id);

                embed.AddField(":first_place:", $"{firstUser.Username} ({firstUser.Mention}) - {firstGlobalUser.Cash:C0}");
                embed.AddField(":second_place:", $"{secondUser.Username} ({secondUser.Mention}) - {secondGlobalUser.Cash:C0}");
                embed.AddField(":third_place:", $"{thirdUser.Username} ({thirdUser.Mention}) - {thirdGlobalUser.Cash:C0}");

                IEnumerable<string> remainingUsers = economyUsers.Skip(3)
                    .Select(u =>
                    {
                        DiscordUser? user = ctx.Client.GetUserAsync(u.Id).GetAwaiter().GetResult();
                        return $"{user.Username} ({user.Mention}) - {u.Cash:C0}";
                    });

                foreach (var user in remainingUsers)
                    embed.AddField($"{position++}.", user);

            }
            else
            {
                IEnumerable<string> formattedUsers = economyUsers.Select(u =>
                {
                    DiscordUser? user = ctx.Client.GetUserAsync(u.Id).GetAwaiter().GetResult();
                    return $"{user.Username} ({user.Mention}) - ${u.Cash}";
                });

                foreach (var user in formattedUsers)
                    embed.AddField($"{position++}.", user);
            }

            await ctx.RespondAsync(embed);
        }
    }
}