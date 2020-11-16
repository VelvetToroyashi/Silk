using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using SilkBot.Utilities;
using System.Linq;
using System.Threading.Tasks;

namespace SilkBot.Commands.Moderation.SClean
{
    [Category(Categories.Mod)]
    [Group("SClean")]
    public partial class SCleanCommand : BaseCommandModule
    {
        [Command, HelpDescription("Clean messages of a specific type, or from specific people!")]
        public async Task SClean(CommandContext ctx)
        {
            using var db = _dbFactory.CreateDbContext();
            var prefix = db.Guilds.First(g => g.Id == ctx.Guild.Id);
            await ctx.RespondAsync($"Are you looking for `{prefix.Prefix}help SClean`?");
        }
    }
}
