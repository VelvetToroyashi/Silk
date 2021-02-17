using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Core.Database.Models;
using Silk.Core.Services.Interfaces;
using Silk.Core.Utilities;
using Silk.Core.Utilities.HelpFormatter;

namespace Silk.Core.Commands.Server.Configuration
{

    [Experimental]
    [RequireGuild]
    [Group("config")]
    [Aliases("configuration")]
    [Category(Categories.Server)]
    [RequireUserPermissions(Permissions.ManageGuild)]
    [Description("Edit configurations the caveman way!\nOr perhaps we just haven't launched the dashboard yet..")]
    public partial class BaseConfigCommand : BaseCommandModule
    {
        private readonly IDatabaseService _db;

        public BaseConfigCommand(IDatabaseService db)
        {
            _db = db;
        }
        
        [GroupCommand]
        public async Task Config(CommandContext ctx)
        {
            GuildConfig config = await _db.GetConfigAsync(ctx.Guild.Id);
            
        }
    }
}