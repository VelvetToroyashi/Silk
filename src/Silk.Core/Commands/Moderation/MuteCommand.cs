using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Core.Data.Models;
using Silk.Core.Services.Interfaces;
using Silk.Core.Types;
using Silk.Core.Utilities;
using Silk.Core.Utilities.HelpFormatter;
using Silk.Extensions;
using Silk.Extensions.DSharpPlus;

namespace Silk.Core.Commands.Moderation
{
    [Category(Categories.Mod)]
    public class MuteCommand : BaseCommandModule
    {
        private readonly IInfractionService _infractions;
        
        public MuteCommand(IInfractionService infractions)
        {
            _infractions = infractions;
        }

        [Command("mute")]
        [RequireFlag(UserFlag.Staff)]
        [RequirePermissions(Permissions.ManageRoles)]
        [Description("Mute a guild member!")]
        [Priority(0)]
        public async Task Mute(CommandContext ctx, DiscordMember user, [RemainingText] string reason = "Not Given.")
        {
            DiscordMember bot = ctx.Guild.CurrentMember;

            if (user.IsAbove(bot))
            {
                int roleDiff = user.Roles.Max(r => r.Position) - ctx.Guild.CurrentMember.Roles.Max(r => r.Position);
                string message;

                message = roleDiff is not 0 ?
                    $"I can't do that! They're {roleDiff} role(s) above me!" :
                    "We have the same top role! I can't add roles to this person.";

                await ctx.RespondAsync(message);
                return;
            }

            if (user.IsAbove(ctx.Member))
            {
                int roleDiff = user.Roles.Max(r => r.Position) - ctx.Member.Roles.Max(r => r.Position);
                string message;

                message = roleDiff is not 0 ?
                    $"I can't do that! They're {roleDiff} role(s) above you!" :
                    "You two see eye to eye! c: I can't mute someone with the same role as you.";

                await ctx.RespondAsync(message);
                return;
            }
            
            var res = await _infractions.MuteAsync(user.Id, ctx.Guild.Id, ctx.User.Id, reason, null);
            var msg = res switch
            {
                InfractionResult.SucceededWithNotification => $"🔇 Muted **{user.ToDiscordName()}** indefinitely! (User notified with Direct Message).",
                InfractionResult.SucceededWithoutNotification => $"🔇 Muted **{user.ToDiscordName()}** indefinitely! (Failed to DM).",
                InfractionResult.FailedGuildMemberCache => $"🔇 Muted **{user.ToDiscordName()}** indefinitely! (Member left server)."
            };
            await ctx.RespondAsync(msg);
        }

        [Priority(1)]
        [Command("mute")]
        [RequireFlag(UserFlag.Staff)]
        public async Task TempMute(CommandContext ctx, DiscordMember user, TimeSpan duration, [RemainingText] string reason = "Not Given.")
        {
            DiscordMember bot = ctx.Guild.CurrentMember;

            if (user.IsAbove(bot))
            {
                int roleDiff = user.Roles.Max(r => r.Position) - ctx.Guild.CurrentMember.Roles.Max(r => r.Position);
                string message;

                message = roleDiff is not 0 ?
                    $"I can't do that! They're {roleDiff} role(s) above me!" :
                    "We have the same top role! I can't add roles to this person.";

                await ctx.RespondAsync(message);
                return;
            }

            if (user.IsAbove(ctx.Member))
            {
                int roleDiff = user.Roles.Max(r => r.Position) - ctx.Member.Roles.Max(r => r.Position);
                string message;

                message = roleDiff is not 0 ?
                    $"I can't do that! They're {roleDiff} role(s) above you!" :
                    "You two see eye to eye! c: I can't mute someone with the same role as you.";

                await ctx.RespondAsync(message);
                return;
            }

            var res = await _infractions.MuteAsync(user.Id, ctx.Guild.Id, ctx.User.Id, reason, null);
            var msg = res switch
            {
                InfractionResult.SucceededWithNotification => $"🔇 Muted **{user.ToDiscordName()}**! Mute expires {Formatter.Timestamp(duration)} (User notified with Direct Message).",
                InfractionResult.SucceededWithoutNotification => $"🔇 Muted **{user.ToDiscordName()}**! Mute expires {Formatter.Timestamp(duration)} (Failed to DM).",
                InfractionResult.FailedGuildMemberCache => $"🔇 Muted **{user.ToDiscordName()}**! Mute expires {Formatter.Timestamp(duration)} (Member left server)."
            };
            await ctx.RespondAsync(msg);
        }
    }
}