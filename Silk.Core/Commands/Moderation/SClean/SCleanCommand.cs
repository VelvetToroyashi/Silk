using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Silk.Core.Database;
using Silk.Core.Database.Models;
using Silk.Core.Utilities;

namespace Silk.Core.Commands.Moderation.SClean
{
    [Category(Categories.Mod)]
    [Group("SClean")]
    public partial class SCleanCommand : BaseCommandModule
    {
        [Command]
        [HelpDescription("Clean messages of a specific type, or from specific people!")]
        public async Task SClean(CommandContext ctx)
        {
            using SilkDbContext db = _dbFactory.CreateDbContext();
            GuildModel prefix = db.Guilds.First(g => g.Id == ctx.Guild.Id);
            await ctx.RespondAsync($"Are you looking for `{prefix.Prefix}help SClean`?");
        }
    }
}