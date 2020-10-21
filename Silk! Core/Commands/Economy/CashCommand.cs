using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using SilkBot.Utilities;

namespace SilkBot.Commands.Economy
{
    public class CashCommand : BaseCommandModule
    {
        [Command("Cash")]
        [Aliases("Money", "mons", "mon", "monni", "moni", "Coins", "Tokens")]
        public async Task Cash(CommandContext ctx)
        {
            var account = SilkBot.Bot.Instance.SilkDBContext.Users.FirstOrDefault(u => u.UserId == ctx.User.Id);
            account ??= new Models.UserInfoModel { UserId = ctx.User.Id, Cash = 200 };
            await SilkBot.Bot.Instance.SilkDBContext.SaveChangesAsync();

            var eb = EmbedHelper.CreateEmbed(ctx, "Account balance:", $"You have {account.Cash} dollars!").WithAuthor(name: ctx.User.Username, iconUrl: ctx.User.AvatarUrl);
            await ctx.RespondAsync(embed: eb);
        }
    }
}
