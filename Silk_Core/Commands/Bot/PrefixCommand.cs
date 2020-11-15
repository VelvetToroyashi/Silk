using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.EntityFrameworkCore;
using SilkBot.Models;
using SilkBot.Services;
using SilkBot.Utilities;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static SilkBot.Bot;

namespace SilkBot.Commands.Bot
{
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class PrefixCommand : BaseCommandModule
    {
        private const int PrefixMaxLength = 5;
        private readonly PrefixCacheService _prefixCache;
        private readonly IDbContextFactory<SilkDbContext> _dbFactory;


        public PrefixCommand(PrefixCacheService prefixCache, IDbContextFactory<SilkDbContext> dbFactory) { _prefixCache = prefixCache; _dbFactory = dbFactory; }

        [Command("setprefix"), RequireFlag(UserFlag.Staff)]
        public async Task SetPrefix(CommandContext ctx, string prefix)
        {
            var db = _dbFactory.CreateDbContext();
            var config = db.Guilds.FirstOrDefault(g => g.Id == ctx.Guild.Id);

            var (valid, reason) = IsValidPrefix(prefix);
            if (!valid)
            {
                await ctx.RespondAsync(reason);
                return;
            }
            
            GuildModel guild = db.Guilds.First(g => g.Id == ctx.Guild.Id);
            guild.Prefix = prefix;
            _prefixCache.UpdatePrefix(ctx.Guild.Id, prefix);
            await db.SaveChangesAsync();
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
            var prefix = _prefixCache.RetrievePrefix(ctx.Guild?.Id);

            await ctx.RespondAsync($"My prefix is `{prefix}`, but you can always use commands by mentioning me! ({ctx.Client.CurrentUser.Mention})");
        }
    }
}