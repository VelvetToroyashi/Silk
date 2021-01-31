using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Core.Database.Models;
using Silk.Core.Services.Interfaces;
using Silk.Core.Utilities;

namespace Silk.Core.Commands.Moderation
{
    [Experimental]
    public class CasesCommand : BaseCommandModule
    {
        private readonly IDatabaseService _dbService;
        
        public CasesCommand(IDatabaseService dbService)
        {
            _dbService = dbService;
        }
        
        [Command]
        public async Task Cases(CommandContext ctx, DiscordUser user)
        {
            var mBuilder = new DiscordMessageBuilder();
            var eBuilder = new DiscordEmbedBuilder();
            
            User? userModel = await _dbService.GetGuildUserAsync(ctx.Guild.Id, user.Id);
            if (userModel is null || !userModel.Infractions.Any())
            {
                mBuilder.WithReply(ctx.Message.Id);
                mBuilder.WithContent("User has no cases!");
                await ctx.RespondAsync(mBuilder);
            }
        }
    }
}