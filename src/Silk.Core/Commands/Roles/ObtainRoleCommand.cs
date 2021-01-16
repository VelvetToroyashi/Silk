using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Core.Services;
using Silk.Core.Services.Interfaces;
using Silk.Core.Utilities;

namespace Silk.Core.Commands.Roles
{
    [Category(Categories.Misc)]
    public class ObtainRoleCommand : BaseCommandModule
    {
        private readonly IDatabaseService _dbService;
        
        public ObtainRoleCommand(IDatabaseService dbService)
        {
            _dbService = dbService;
        }

        [Command("get_role")]
        public async Task ObtainRole(CommandContext ctx, params DiscordRole[] roles)
        {
            
        }
        
    }
}