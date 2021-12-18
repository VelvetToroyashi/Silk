/*using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Services.Interfaces;
using Silk.Types;
using Silk.Utilities.HelpFormatter;
using Silk.Extensions.DSharpPlus;

namespace Silk.Commands.Moderation
{
    [ExcludeFromSlashCommands]
    [HelpCategory(Categories.Mod)]
    public class UnbanCommand : BaseCommandModule
    {
        private readonly IInfractionService _infractions;
        public UnbanCommand(IInfractionService infractions) => _infractions = infractions;

        [Command("unban")]
        [RequireBotPermissions(Permissions.BanMembers)]
        [RequireUserPermissions(Permissions.BanMembers)]
        [Description("Un-bans someone from the current server!")]
        public async Task UnBan(CommandContext ctx, DiscordUser user, [RemainingText] string reason = "Not Given.")
        {
            InfractionResult res = await _infractions.UnBanAsync(user.Id, ctx.Guild.Id, ctx.User.Id, reason);
            string? message = res switch
            {
                InfractionResult.SucceededDoesNotNotify => $"Unbanned **{user.ToDiscordName()}**!",
                InfractionResult.FailedResourceDeleted  => "That member doesn't appear to be banned!"
            };

            await ctx.RespondAsync(message);
        }
    }
}*/
