using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Core.Database.Models;
using Silk.Core.Services.Interfaces;

namespace Silk.Core.Economy
{
    public class FlipCommand : BaseCommandModule
    {
        private readonly IDatabaseService _dbService;
        
        public FlipCommand(IDatabaseService dbService)
        {
            _dbService = dbService;
        }
        
        [Command]
        [Description("Flip a metaphorical coin, and double your proffits, or lose everything~")]
        public async Task FlipAsync(CommandContext ctx, uint amount)
        {
            DiscordMessageBuilder builder = new();
            builder.WithReply(ctx.Message.Id);
            
            GlobalUser user = await _dbService.GetOrCreateGlobalUserAsync(ctx.User.Id);
            Random ran = new((int)ctx.Message.Id);
            bool won;

            var nextRan = ran.Next(10000);

            won = nextRan % 20 > 2;
            
            
        }
    }
}