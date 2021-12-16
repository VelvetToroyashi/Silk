/*using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Data.Entities;
using Silk.Services.Interfaces;
using Silk.Types;
using Silk.Utilities;
using Silk.Utilities.HelpFormatter;
using Silk.Extensions.DSharpPlus;

namespace Silk.Commands.Moderation
{
    [HelpCategory(Categories.Mod)]
    public class UnMuteCommand : BaseCommandModule
    {
        private readonly IInfractionService _infractions;
        public UnMuteCommand(IInfractionService infractions) => _infractions = infractions;

        [Command]
        
        [Description("Un-mutes a member.")]
        public async Task UnmuteAsync(CommandContext ctx, DiscordUser user, [RemainingText] string reason = "Not Given.")
        {
            if (user == ctx.User)
            {
                await ctx.RespondAsync("Sorry, but even if you were muted, I couldn't let you do that!");
                return;
            }

            InfractionResult res = await _infractions.UnMuteAsync(user.Id, ctx.Guild.Id, ctx.User.Id, reason);

            string? message = res switch
            {
                InfractionResult.FailedGenericRequirementsNotFulfilled => "That person isn't muted!",
                InfractionResult.SucceededWithNotification             => $"Un-muted **{user.ToDiscordName()}**! (User notified with Direct Message).",
                _                                                      => throw new ArgumentOutOfRangeException()
            };

            await ctx.RespondAsync(message);
        }
    }
}*/
