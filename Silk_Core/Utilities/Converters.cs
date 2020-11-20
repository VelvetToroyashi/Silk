using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using SilkBot.Extensions;

namespace SilkBot.Utilities
{
    public class MemberConverter : IArgumentConverter<DiscordMember>
    {
        public async Task<Optional<DiscordMember>> ConvertAsync(string value, CommandContext ctx)
        {
            if (ctx.Guild.Members.Values.Any(m => m.Username.Contains(value, StringComparison.OrdinalIgnoreCase) || (m.Nickname?.Contains(value, StringComparison.OrdinalIgnoreCase) ?? false)))
                return Optional.FromValue(ctx.Guild.Members.Values.First(m => (m.Nickname ?? m.Username).Contains(value, StringComparison.OrdinalIgnoreCase)));
            else
            {

                typeof(DiscordMemberConverter).GetMethods().First(m => m.Name == "ConvertAsync")
                    .Invoke(null, BindingFlags.CreateInstance, null, new object[] {value, ctx}, CultureInfo.CurrentCulture);
                // if (value.StartsWith("<@"))
                // {
                //     await ctx.Guild.GetMemberAsync(ulong.Parse(value.SkipWhile(c => c is <= '9' and >= '0').ToArray()[..^1].JoinString(null)));
                // }
            }
            
            return Optional.FromNoValue<DiscordMember>();
        }
    }

}
