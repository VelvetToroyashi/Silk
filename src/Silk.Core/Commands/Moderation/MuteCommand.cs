using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Core.Database.Models;
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
        private readonly ConfigService _dbService;
        private readonly IInfractionService _infractionService;

        public MuteCommand(ConfigService dbService, IInfractionService infractionService)
        {
            _dbService = dbService;
            _infractionService = infractionService;
        }

        [Command("mute")]
        [RequirePermissions(Permissions.ManageRoles)]
        [Description("Temporarily mute a guild member")]
        public async Task TempMute(CommandContext ctx, DiscordMember user, TimeSpan duration, [RemainingText] string reason = "Not Given.")
        {
            DiscordMember bot = ctx.Guild.CurrentMember;
            if (user.IsAbove(bot))
            {
                await ctx.RespondAsync($"{user.Username} is {user.Roles.Last().Position - bot.Roles.Last().Position} roles above me!")
                    .ConfigureAwait(false);
                return;
            }

            GuildConfig config = (await _dbService.GetConfigAsync(ctx.Guild.Id))!;

            if (config.MuteRoleId is 0)
            {
                await ThrowHelper.MuteRoleNotFoundInDatabase(ctx.Channel);
                return;
            }

            Infraction infraction = await _infractionService.CreateTemporaryInfractionAsync(user, ctx.Member,
                InfractionType.Mute, reason, DateTime.Now.Add(duration));

            await _infractionService.MuteAsync(user, ctx.Channel, infraction);
        }
    }
}