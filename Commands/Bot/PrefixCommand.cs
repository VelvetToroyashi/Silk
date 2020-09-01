using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using SilkBot.Models;
using SilkBot.Utilities;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static SilkBot.Bot;

namespace SilkBot.Commands.Bot
{
    public class PrefixCommand : BaseCommandModule
    {
        [Command("Prefix")]
        [Aliases("SetPrefix")]
        public async Task SetPrefix(CommandContext ctx, string prefix)
        {

            var config = Instance.DbContext.Guilds.FirstOrDefault(g => g.DiscordGuildId == ctx.Guild.Id);
            if (!config.DiscordUserInfos.Any(user => user.UserPermissions.HasFlag(UserPrivileges.Staff)))
            {
                await ctx.RespondAsync("Sorry, but you're not allowed to change the prefix!");
                return;
            }
            var (valid, reason) = IsValidPrefix(prefix);
            if (!valid)
            {
                await ctx.RespondAsync(reason);
                return;
            }

            SilkBot.Bot.Instance.DbContext.Guilds.FirstOrDefault(g => g.DiscordGuildId == ctx.Guild.Id).Prefix = prefix;
            await SilkBot.Bot.Instance.DbContext.SaveChangesAsync();
            await ctx.RespondAsync($"Done! I'll respond to `{prefix}` from now on.");
        }
        private (bool valid, string reason) IsValidPrefix(string prefix)
        {
            if (prefix.Length > 5)
                return (false, "Prefix cannot be more than 5 characters!");
            if (!Regex.IsMatch(prefix, "[A-Z!@#$%^&*<>?.]+", RegexOptions.IgnoreCase)) return (false, "Invalid prefix! `[Valid symbols: ! @ # $ % ^ & * < > ? / and A-Z (Case insensitive)]`");
            else return (true, "");
        }

        [Command("Prefix")]
        public async Task SetPrefix(CommandContext ctx)
        {
            await ctx.RespondAsync($"My prefix is `{SilkBot.Bot.GuildPrefixes[ctx.Guild.Id]}`, but you can always use commands by mentioning me! ({ctx.Client.CurrentUser.Mention})");
        }


    }
}
