using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.EntityFrameworkCore;
using SilkBot.Models;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static SilkBot.Bot;

namespace SilkBot.Commands.Bot
{
    public class PrefixCommand : BaseCommandModule
    {
        private const int PrefixMaxLength = 5;
        
        [Command("Prefix")]
        [Aliases("SetPrefix")]
        public async Task SetPrefix(CommandContext ctx, string prefix)
        {
            var config = Instance.SilkDBContext.Guilds.FirstOrDefault(g => g.DiscordGuildId == ctx.Guild.Id);
            if (!config.DiscordUserInfos.Any(user => user.Flags.HasFlag(UserFlag.Staff)))
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

            var currentGuild = Instance.SilkDBContext.Guilds.FirstOrDefault(g => g.DiscordGuildId == ctx.Guild.Id);
            currentGuild.Prefix = prefix;
            
            await Instance.SilkDBContext.SaveChangesAsync();
            await ctx.RespondAsync($"Done! I'll respond to `{prefix}` from now on.");
        }

        private PrefixValidationResult IsValidPrefix(string prefix)
        {
            if (prefix.Length > PrefixMaxLength)
            {
                return new PrefixValidationResult { Reason = $"Prefix cannot be more than {PrefixMaxLength} characters!" };
            }

            if (!Regex.IsMatch(prefix, "[A-Z!@#$%^&*<>?.]+", RegexOptions.IgnoreCase))
            {
                return new PrefixValidationResult { Reason = "Invalid prefix! `[Valid symbols: ! @ # $ % ^ & * < > ? / and A-Z (Case insensitive)]`" };
            }

            return new PrefixValidationResult { Valid = true };
        }

        [Command("Prefix")]
        public async Task SetPrefix(CommandContext ctx)
        {
            await ctx.RespondAsync($"My prefix is `{new SilkDbContext().Guilds.FirstOrDefault(g => g.DiscordGuildId == ctx.Guild.Id).Prefix}`, but you can always use commands by mentioning me! ({ctx.Client.CurrentUser.Mention})");
        }
    }
}