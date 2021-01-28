using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Silk.Core.Database;
using Silk.Core.Database.Models;
using Silk.Core.Utilities;

namespace Silk.Core.Commands.Economy
{
    [Category(Categories.Economy)]
    public class BalTopCommand : BaseCommandModule
    {
        private readonly IDbContextFactory<SilkDbContext> _dbFactory;
        private readonly ILogger<BalTopCommand> _logger;

        public BalTopCommand(
            IDbContextFactory<SilkDbContext> dbContextFactory,
            ILogger<BalTopCommand> logger)
        {
            _dbFactory = dbContextFactory;
            _logger = logger;
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

            bool addedTopMember = false;
            int position = 0;

            for (var i = 0; i < economyUsers.Count; ++i)
            {
                GlobalUserModel? economyUser = economyUsers[i];

                bool exceptionOccurred = false;
                DiscordUser? ctxUser = null;

                try
                {
                    ctxUser = await ctx.Client.GetUserAsync(economyUser.Id);
                }
                catch (Exception e)
                {
                    exceptionOccurred = true;
                    _logger.LogInformation(e.Message);
                }
                
                if (ctxUser is null || exceptionOccurred) continue;
                
                position++;

                var displayName = ctxUser.Username;
                var fieldHeader = addedTopMember == false ? $"{position}. :crown:" : $"{position}. ";
                var fieldValue = $"{displayName} - ${economyUser.Cash}";
                
                embed.AddField(fieldHeader, fieldValue);
                
                addedTopMember = true;
            }

            await ctx.RespondAsync(embed);
        }
    }
}