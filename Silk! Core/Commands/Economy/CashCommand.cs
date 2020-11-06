using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.EntityFrameworkCore;
using SilkBot.Extensions;

namespace SilkBot.Commands.Economy
{
    public class CashCommand : BaseCommandModule
    {
        public IDbContextFactory<SilkDbContext> DbContextFactory { private get; set; }
        [Command("Cash")]
        [Aliases("Money")]
        public async Task Cash(CommandContext ctx)
        {
            using var db = DbContextFactory.CreateDbContext();
            var account = db.Users.FirstOrDefault(u => u.UserId == ctx.User.Id);
            account ??= new Models.UserInfoModel { UserId = ctx.User.Id, Cash = 200 };
            await db.SaveChangesAsync();

            var eb = EmbedHelper.CreateEmbed(ctx, "Account balance:", $"You have {account.Cash} dollars!").WithAuthor(name: ctx.User.Username, iconUrl: ctx.User.AvatarUrl);
            await ctx.RespondAsync(embed: eb);
        }
    }
}
