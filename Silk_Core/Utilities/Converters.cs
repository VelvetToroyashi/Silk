using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SilkBot.Utilities
{
    public class MemberConverter : IArgumentConverter<DiscordMember>
    {
        public Task<Optional<DiscordMember>> ConvertAsync(string value, CommandContext ctx)
        {
            if (ctx.Guild.Members.Values.Any(m => m.Username.Contains(value, StringComparison.OrdinalIgnoreCase) || (m.Nickname?.Contains(value, StringComparison.OrdinalIgnoreCase) ?? false)))
                return Task.FromResult(Optional.FromValue<DiscordMember>(ctx.Guild.Members.Values.First(m => (m.Nickname ?? m.Username).Contains(value, StringComparison.OrdinalIgnoreCase))));
            else return Task.FromResult(Optional.FromNoValue<DiscordMember>());
        }
    }

}
