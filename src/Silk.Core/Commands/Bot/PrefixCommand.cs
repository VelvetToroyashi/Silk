using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Silk.Core.Data;
using Silk.Core.Data.Models;
using Silk.Core.Services.Interfaces;
using Silk.Core.Utilities.HelpFormatter;
using Silk.Extensions;
using Silk.Shared.Constants;

namespace Silk.Core.Commands.Bot
{
    [Category(Categories.Bot)]
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class PrefixCommand : BaseCommandModule
    {
        private const int PrefixMaxLength = 5;
        private readonly GuildContext _db;
        private readonly IPrefixCacheService _prefixCache;

        public PrefixCommand(IPrefixCacheService prefixCache, GuildContext db)
        {
            _prefixCache = prefixCache;
            _db = db;
        }

        [Command("prefix")]
        [Description("Sets the command prefix for Silk to use on the current Guild")]
        public async Task SetPrefix(CommandContext ctx, string prefix)
        {
            if (!ctx.Member.HasPermission(FlagConstants.CacheFlag)) return;

            (bool valid, string reason) = IsValidPrefix(prefix);
            if (!valid)
            {
                await ctx.RespondAsync(reason);
                return;
            }

            Guild guild = _db.Guilds.First(g => g.Id == ctx.Guild.Id);
            guild.Prefix = prefix;
            _prefixCache.UpdatePrefix(ctx.Guild.Id, prefix);

            await _db.SaveChangesAsync();
            await ctx.RespondAsync($"Done! I'll respond to `{prefix}` from now on.");
        }

        [Command("prefix")]
        [Description("Gets Silk's command prefix for the current Guild")]
        public async Task GetPrefix(CommandContext ctx)
        {
            string prefix = _prefixCache.RetrievePrefix(ctx.Guild?.Id);
            await ctx.RespondAsync($"My prefix is `{prefix}`, but you can always use commands by mentioning me! ({ctx.Client.CurrentUser.Mention})");
        }

        private PrefixValidationResult IsValidPrefix(string prefix)
        {
            if (prefix.Length > PrefixMaxLength)
                return new(false, $"Prefix cannot be more than {PrefixMaxLength} characters!");

            if (!Regex.IsMatch(prefix, "[A-Z!@#$%^&*<>?.]+", RegexOptions.IgnoreCase))
                return new(false, "Invalid prefix! `[Valid symbols: ! @ # $ % ^ & * < > ? / and A-Z (Case insensitive)]`");

            return new(true, "");
        }

        private record PrefixValidationResult(bool Valid, string Reason);
    }
}