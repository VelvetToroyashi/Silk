using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Core.Data.Models;
using Silk.Core.Discord.Services;
using Silk.Core.Discord.Services.Interfaces;
using Silk.Core.Discord.Utilities;
using Silk.Core.Discord.Utilities.HelpFormatter;
using Silk.Extensions;
using Silk.Shared.Abstractions.DSharpPlus.Interfaces;

namespace Silk.Core.Logic.Commands.Moderation
{
    [Category(Categories.Mod)]
    public class MuteCommand : BaseCommandModule
    {
        private readonly ConfigService _configService;
        private readonly IInfractionService _infractionService;
        private readonly IMessageSender _sender;

        public MuteCommand(ConfigService configService, IInfractionService infractionService, IMessageSender sender)
        {
            _configService = configService;
            _infractionService = infractionService;
            _sender = sender;
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
                var roleDiff = user.Roles.Max(r => r.Position) - ctx.Member.Roles.Max(r => r.Position);
                string message;

                message = roleDiff is not 0 ?
                    $"I can't do that! They're {roleDiff} role(s) above me!" :
                    "We have the same top role! I can't add roles to this person.";

                await _sender.SendAsync(ctx.Channel.Id, message);
                return;
            }

            if (user.IsAbove(ctx.Member))
            {
                var roleDiff = user.Roles.Max()!.Position - ctx.Member.Roles.Max()!.Position;
                string message;

                message = roleDiff is not 0 ?
                    $"I can't do that! They're {roleDiff} role(s) above you!" :
                    "You two see eye to eye! c: I can't mute someone with the same role as you.";

                await _sender.SendAsync(ctx.Channel.Id, message);
                return;
            }

            if (config.MuteRoleId is 0)
            {
                await ThrowHelper.MisconfiguredMuteRole(ctx.Channel.Id, _sender);
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
                var roleDiff = user.Roles.Max(r => r.Position) - ctx.Member.Roles.Max(r => r.Position);
                string message;

                message = roleDiff is not 0 ?
                    $"I can't do that! They're {roleDiff} role(s) above me!" :
                    "We have the same top role! I can't add roles to this person.";

                await _sender.SendAsync(ctx.Channel.Id, message);
                return;
            }

            if (user.IsAbove(ctx.Member))
            {
                var roleDiff = user.Roles.Max()!.Position - ctx.Member.Roles.Max()!.Position;
                string message;

                message = roleDiff is not 0 ?
                    $"I can't do that! They're {roleDiff} role(s) above you!" :
                    "You two see eye to eye! c: I can't mute someone with the same role as you.";

                await _sender.SendAsync(ctx.Channel.Id, message);
                return;
            }

            GuildConfig config = (await _configService.GetConfigAsync(ctx.Guild.Id))!;

            if (config.MuteRoleId is 0)
            {
                await ThrowHelper.MisconfiguredMuteRole(ctx.Channel.Id, _sender);
                return;
            }

            Infraction infraction = await _infractionService.CreateTempInfractionAsync(user, ctx.Member,
                InfractionType.Mute, reason, DateTime.Now.Add(duration));

            await _infractionService.MuteAsync(user, ctx.Channel, infraction);
            await ctx.RespondAsync($":white_check_mark: Muted {user.Username}.");
        }
    }
}