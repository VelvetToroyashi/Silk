using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Core.Utilities.HelpFormatter;

namespace Silk.Core.Commands.Moderation.Ban
{
    [Category(Categories.Mod)]
    public class TempBanCommand : BaseCommandModule
    {
        private const string defaultFormat = "$mention was $action from the guild for {d} for {reason}";

        [Hidden]
        [RequireGuild]
        [Command("tempban")]
        [Description("Temporarily ban a member from the Guild")]
        public async Task TempBan(CommandContext ctx, DiscordMember user, TimeSpan duration, [RemainingText] string reason = "Not provided.")
        {
           
        }
    }
}