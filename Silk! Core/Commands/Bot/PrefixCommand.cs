using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.EntityFrameworkCore;
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
        private const int PrefixMaxLength = 5;
        
        [Command("Prefix")]
        [Aliases("SetPrefix")]
        [RequireFlag(UserFlag.Staff)]
        public async Task SetPrefix(CommandContext ctx, string prefix)
        {
            var config = Instance.SilkDBContext.Guilds.FirstOrDefault(g => g.DiscordGuildId == ctx.Guild.Id);

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
            using var db = new SilkDbContext();
            var prefix = db.Guilds.FirstOrDefault(g => g.DiscordGuildId == ctx.Guild.Id)?.Prefix
                         ?? SilkDefaultCommandPrefix;

            await ctx.RespondAsync($"My prefix is `{prefix}`, but you can always use commands by mentioning me! ({ctx.Client.CurrentUser.Mention})");
        }
    }
}