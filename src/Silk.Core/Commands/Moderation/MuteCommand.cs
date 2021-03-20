using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Core.Data.Models;
using Silk.Core.Services;
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
        [RequirePermissions(Permissions.ManageRoles)]
        [Description("Mute a guild member!")]
        [Priority(0)]
        public async Task Mute(CommandContext ctx, DiscordMember user, [RemainingText] string reason = "Not Given.")
        {
            DiscordMember bot = ctx.Guild.CurrentMember;
            GuildConfig config = (await _configService.GetConfigAsync(ctx.Guild.Id))!;
            if (user.IsAbove(bot))
            {
                await ctx.RespondAsync($"{user.Username} is {user.Roles.Last().Position - bot.Roles.Last().Position} role(s) above me!");
                return;
            }
            if (user.IsAbove(ctx.Member) && user.Roles.All(r => r.Id != config.MuteRoleId))
            {
                await ctx.RespondAsync($"They're {user.Roles.Last().Position - ctx.Member.Roles.Last().Position} role(s) above you!");
                return;
            }
            
            if (config.MuteRoleId is 0)
            {
                await ThrowHelper.MisconfiguredMuteRole(ctx.Channel);
                return;
            }

            Infraction infraction = await _infractionService.CreateTempInfractionAsync(user, ctx.Member,
                InfractionType.Mute, reason);

            await _infractionService.MuteAsync(user, ctx.Channel, infraction);
            await ctx.RespondAsync($":white_check_mark: Muted {user.Username} indefinitely.");
        }
        
        [Command("mute")]
        [Priority(1)]
        public async Task TempMute(CommandContext ctx, DiscordMember user, TimeSpan duration, [RemainingText] string reason = "Not Given.")
        {
            DiscordMember bot = ctx.Guild.CurrentMember;
            if (user.IsAbove(bot))
            {
                await ctx.RespondAsync($"{user.Username} is {user.Roles.Last().Position - bot.Roles.Last().Position} role(s) above me!")
                    .ConfigureAwait(false);
                return;
            }
            if (user.IsAbove(ctx.Member))
            {
                await ctx.RespondAsync($"They're {user.Roles.Last().Position - ctx.Member.Roles.Last().Position} role(s) above you!");
                return;
            }

            GuildConfig config = (await _configService.GetConfigAsync(ctx.Guild.Id))!;

            if (config.MuteRoleId is 0)
            {
                await ThrowHelper.MisconfiguredMuteRole(ctx.Channel);
                return;
            }

            Infraction infraction = await _infractionService.CreateTempInfractionAsync(user, ctx.Member,
                InfractionType.Mute, reason, DateTime.Now.Add(duration));

            await _infractionService.MuteAsync(user, ctx.Channel, infraction);
            await ctx.RespondAsync($":white_check_mark: Muted {user.Username}.");
        }
    }
}