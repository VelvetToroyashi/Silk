using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;

namespace Silk.Core.Utilities.Bot
{
    public class MemberConverter : IArgumentConverter<DiscordMember>
    {
        private static Regex UserRegex { get; } =
            new(@"^<@\!?(\d+?)>$", RegexOptions.ECMAScript | RegexOptions.Compiled);

        public async Task<Optional<DiscordMember>> ConvertAsync(string value, CommandContext ctx)
        {

            var user = ctx.Guild?.Members.Values.FirstOrDefault(m =>
                m.Nickname?.Contains(value, StringComparison.OrdinalIgnoreCase) ?? m.Username.Contains(value, StringComparison.OrdinalIgnoreCase));
            if (user is not null) return Optional.FromValue(user);
            return await ConvertMemberAsync(value, ctx);
        }


        // Basically ripped from the source since we can't call this from the built-in one *shrug*

        private static async Task<Optional<DiscordMember>> ConvertMemberAsync(string value, CommandContext ctx)
        {
            if (ctx.Guild == null)
                return Optional.FromNoValue<DiscordMember>();

            if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong uid))
            {
                DiscordMember result = await ctx.Guild.GetMemberAsync(uid).ConfigureAwait(false);
                Optional<DiscordMember> ret =
                    result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordMember>();
                return ret;
            }

            Match m = UserRegex.Match(value);
            if (m.Success && ulong.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture,
                out uid))
            {
                DiscordMember result = await ctx.Guild.GetMemberAsync(uid).ConfigureAwait(false);
                Optional<DiscordMember> ret =
                    result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordMember>();
                return ret;
            }

            value = value.ToLowerInvariant();

            int di = value.IndexOf('#');
            string un = di != -1 ? value.Substring(0, di) : value;
            string? dv = di != -1 ? value.Substring(di + 1) : null;

            IEnumerable<DiscordMember>? us = ctx.Guild?.Members.Values
                .Where(xm =>
                    xm.Username.ToLowerInvariant() == un &&
                    (dv != null && xm.Discriminator == dv || dv == null)
                    || xm.Nickname?.ToLowerInvariant() == value);

            DiscordMember? mbr = us?.FirstOrDefault();
            return mbr != null ? Optional.FromValue(mbr) : Optional.FromNoValue<DiscordMember>();
        }
    }
}