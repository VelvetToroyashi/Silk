using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;

namespace SilkBot.Utilities
{
    public class MemberConverter : IArgumentConverter<DiscordMember>
    {
        private static Regex UserRegex { get; } =
            new Regex(@"^<@\!?(\d+?)>$", RegexOptions.ECMAScript | RegexOptions.Compiled);

        public async Task<Optional<DiscordMember>> ConvertAsync(string value, CommandContext ctx)
        {
            if (ctx.Guild.Members.Values.Any(m =>
                m.Username.Contains(value, StringComparison.OrdinalIgnoreCase) ||
                (m.Nickname?.Contains(value, StringComparison.OrdinalIgnoreCase) ?? false)))
                return Optional.FromValue(ctx.Guild.Members.Values.First(m =>
                    (m.Nickname ?? m.Username).Contains(value, StringComparison.OrdinalIgnoreCase)));
            return await ConvertMemberAsync(value, ctx);
        }


        // Basically ripped from the source since we can't call this from the built-in one *shrug*

        private static async Task<Optional<DiscordMember>> ConvertMemberAsync(string value, CommandContext ctx)
        {
            if (ctx.Guild == null)
                return Optional.FromNoValue<DiscordMember>();

            if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var uid))
            {
                var result = await ctx.Guild.GetMemberAsync(uid).ConfigureAwait(false);
                var ret = result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordMember>();
                return ret;
            }

            var m = UserRegex.Match(value);
            if (m.Success && ulong.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture,
                out uid))
            {
                var result = await ctx.Guild.GetMemberAsync(uid).ConfigureAwait(false);
                var ret = result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordMember>();
                return ret;
            }

            value = value.ToLowerInvariant();

            var di = value.IndexOf('#');
            var un = di != -1 ? value.Substring(0, di) : value;
            var dv = di != -1 ? value.Substring(di + 1) : null;

            var us = ctx.Guild.Members.Values
                .Where(xm =>
                    xm.Username.ToLowerInvariant() == un && (dv != null && xm.Discriminator == dv || dv == null)
                    || xm.Nickname?.ToLowerInvariant() == value);

            var mbr = us.FirstOrDefault();
            return mbr != null ? Optional.FromValue(mbr) : Optional.FromNoValue<DiscordMember>();
        }
    }
}