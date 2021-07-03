using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.Logging;
using Silk.Core.Data.Models;
using Silk.Core.Utilities;
using Silk.Core.Utilities.HelpFormatter;

namespace Silk.Core.Commands.Miscellaneous
{
    [RequireFlag(UserFlag.Staff)]
    [Category(Categories.Misc)]
    public class NicknameCommand : BaseCommandModule
    {
        private readonly ILogger<NicknameCommand> _logger;

        public NicknameCommand(ILogger<NicknameCommand> logger)
        {
            _logger = logger;
        }
        
        
        [Priority(1)]
        [Aliases("nick")]
        [Command("nickname")]
        [RequirePermissions(Permissions.ManageNicknames)]
        [Description("Set members whom's name matches to the new nickname. This may take a while on large servers.")]
        public async Task SetNickname(CommandContext ctx, string match, [RemainingText] string nickname)
        {
            var members = await ctx.Guild.SearchMembersAsync(match, 1000);
            var skipped = members.Where(m => m.Hierarchy > ctx.Guild.CurrentMember.Hierarchy);
            members = members.Except(skipped).ToArray();
            var reqs = members.Select(mem => mem.ModifyAsync(m => m.Nickname = nickname)).ToArray();

            await ctx.RespondAsync($"Found {members.Count} members, skipping {skipped.Count()} extra. \nEstimated time: {Formatter.Timestamp(TimeSpan.FromMilliseconds(ctx.Client.Ping * reqs.Length))}");

            await Task.WhenAll(reqs);
        }
    }
}