using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Linq;
using static SilkBot.Bot;

namespace SilkBot.Commands.Tests
{
    public class LogInviteCommand : BaseCommandModule
    {
        [Command("whitelistinvites")]
        public async System.Threading.Tasks.Task LogInvite(CommandContext ctx)
        {
            var config = Instance.SilkDBContext.Guilds.AsQueryable().First(g => g.DiscordGuildId == ctx.Guild.Id);
            config.WhiteListInvites = !config.WhiteListInvites;
            await ctx.RespondAsync($"Whitelisting invites: `{(config.WhiteListInvites ? "enabled" : "disabled")}`!");
            await Instance.SilkDBContext.SaveChangesAsync();
        }
    }
}
