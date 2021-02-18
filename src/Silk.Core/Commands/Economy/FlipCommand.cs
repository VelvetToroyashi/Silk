using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Core.Services.Interfaces;
using Silk.Data.Models;

namespace Silk.Core.Commands.Economy
{
    public class FlipCommand : BaseCommandModule
    {
        private readonly IDatabaseService _dbService;
        private readonly string[] _winningMessages =
        {
            "Capitalism shines upon you.",
            "RNG is having a good day, or would it be a bad day?",
            "You defeated all odds, but how far does that luck stretch?",
            "Double the money, but half the luck~ Just kidding ;p"
        };
        private readonly string[] _losingMessages =
        {
            "Yikes!",
            "Better luck next time!",
            "RNG is not your friend, now is it?",
            "Alas, the cost of gambling.",
            "Hope that wasn't all your earnings"
        };


        public FlipCommand(IDatabaseService dbService)
        {
            _dbService = dbService;
        }

        [Command]
        [Cooldown(10, 86400, CooldownBucketType.User)]
        [Description("Flip a metaphorical coin, and double your profits, or lose everything~")]
        public async Task FlipAsync(CommandContext ctx, uint amount)
        {
            DiscordMessageBuilder builder = new();
            DiscordEmbedBuilder embedBuilder = new();
            builder.WithReply(ctx.Message.Id);

            GlobalUser user = await _dbService.GetOrCreateGlobalUserAsync(ctx.User.Id);

            if (amount > user.Cash)
            {
                builder.WithContent("Ah ah ah... You can't gamble more than what you've got in your pockets!");
                await ctx.RespondAsync(builder);
                return;
            }

            Random ran = new((int) ctx.Message.Id);
            bool won;

            int nextRan = ran.Next(1000);

            won = nextRan % 20 > 9;

            if (won)
            {
                embedBuilder.WithColor(DiscordColor.SapGreen)
                    .WithTitle(_winningMessages[ran.Next(_winningMessages.Length)])
                    .WithDescription($"Congragulations! Bet: ${amount}, winnings ${amount * 2}");
                builder.WithEmbed(embedBuilder);
                user.Cash += (int) amount;

            }
            else
            {
                embedBuilder.WithColor(DiscordColor.DarkRed)
                    .WithTitle(_losingMessages[ran.Next(_losingMessages.Length)])
                    .WithDescription("Darn. Seems like you've lost your bet!");
                builder.WithEmbed(embedBuilder);
                user.Cash -= (int) amount;
            }

            await ctx.RespondAsync(builder);
            await _dbService.UpdateGlobalUserAsync(user);
        }
    }
}