using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Silk.Core.Utilities.HelpFormatter;
using Silk.Data;
using Silk.Data.Models;

namespace Silk.Core.Commands.Moderation.SClean
{
    [Hidden]
    [Group("SClean")]
    [Category(Categories.Mod)]
    public partial class SCleanCommand : BaseCommandModule
    {
        [Command]
        [Description("Clean messages of a specific type, or from specific people!")]
        public async Task SClean(CommandContext ctx)
        {
            SilkDbContext db = _dbFactory.CreateDbContext();
            Guild prefix = db.Guilds.First(g => g.Id == ctx.Guild.Id);
            await ctx.RespondAsync($"Are you looking for `{prefix.Prefix}help SClean`?");
        }
    }
}