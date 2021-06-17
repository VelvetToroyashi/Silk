using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Core.Data.Models;
using Silk.Core.Services.Data;
using Silk.Core.Services.Interfaces;
using Silk.Core.Utilities;
using Silk.Core.Utilities.HelpFormatter;
using Silk.Extensions;

namespace Silk.Core.Commands.Moderation
{
    [Category(Categories.Mod)]
    public class MuteCommand : BaseCommandModule
    {
        private readonly ConfigService _configService;
        private readonly IInfractionService _infractionService;


        public MuteCommand(ConfigService configService, IInfractionService infractionService)
        {
            _configService = configService;
            _infractionService = infractionService;
        }

        [Command("mute")]
        [RequireFlag(UserFlag.Staff)]
        [RequirePermissions(Permissions.ManageRoles)]
        [Description("Mute a guild member!")]
        [Priority(0)]
        public async Task Mute(CommandContext ctx, DiscordMember user, [RemainingText] string reason = "Not Given.")
        {
            DiscordMember bot = ctx.Guild.CurrentMember;
            GuildConfig config = (await _configService.GetConfigAsync(ctx.Guild.Id))!;

            if (user.IsAbove(bot))
            {
                int roleDiff = user.Roles.Max(r => r.Position) - ctx.Member.Roles.Max(r => r.Position);
                string message;

                message = roleDiff is not 0 ?
                    $"I can't do that! They're {roleDiff} role(s) above me!" :
                    "We have the same top role! I can't add roles to this person.";

                await ctx.RespondAsync(message);
                return;
            }

            if (user.IsAbove(ctx.Member))
            {
                int roleDiff = user.Roles.Max()!.Position - ctx.Member.Roles.Max()!.Position;
                string message;

                message = roleDiff is not 0 ?
                    $"I can't do that! They're {roleDiff} role(s) above you!" :
                    "You two see eye to eye! c: I can't mute someone with the same role as you.";

                await ctx.RespondAsync(message);
                return;
            }

            if (config.MuteRoleId is 0)
            {
                await ctx.RespondAsync("Mute role isn't configured for this server!"); // TODO: Generate mute role on-demand. 
                return;
            }

            Infraction infraction = await _infractionService.CreateTempInfractionAsync(user, ctx.Member, InfractionType.Mute, reason);

            await _infractionService.MuteAsync(user, ctx.Channel, infraction);
            await ctx.RespondAsync($":white_check_mark: Muted {user.Username} indefinitely.");
        }

        [Priority(1)]
        [Command("mute")]
        [RequireFlag(UserFlag.Staff)]
        public async Task TempMute(CommandContext ctx, DiscordMember user, TimeSpan duration, [RemainingText] string reason = "Not Given.")
        {
            DiscordMember bot = ctx.Guild.CurrentMember;
            if (user.IsAbove(bot))
            {
                int roleDiff = user.Roles.Max(r => r.Position) - ctx.Member.Roles.Max(r => r.Position);
                string message;

                message = roleDiff is not 0 ?
                    $"I can't do that! They're {roleDiff} role(s) above me!" :
                    "We have the same top role! I can't add roles to this person.";

                await ctx.RespondAsync(message);
                return;
            }

            if (user.IsAbove(ctx.Member))
            {
                int roleDiff = user.Roles.Max()!.Position - ctx.Member.Roles.Max()!.Position;
                string message;

                message = roleDiff is not 0 ?
                    $"I can't do that! They're {roleDiff} role(s) above you!" :
                    "You two see eye to eye! c: I can't mute someone with the same role as you.";

                await ctx.RespondAsync(message);
                return;
            }

            GuildConfig config = (await _configService.GetConfigAsync(ctx.Guild.Id))!;

            if (config.MuteRoleId is 0)
            {
                await ctx.RespondAsync("Mute role isn't configured on this server!"); // TODO: Generate mute role on-demand. 
                return;
            }

            Infraction infraction = await _infractionService.CreateTempInfractionAsync(user, ctx.Member,
                InfractionType.Mute, reason, DateTime.Now.Add(duration));

            await _infractionService.MuteAsync(user, ctx.Channel, infraction);
            await ctx.RespondAsync($":white_check_mark: Muted {user.Username}.");
        }
    }
}