using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Core.Database.Models;
using Silk.Core.Services;
using Silk.Core.Services.Interfaces;

namespace Silk.Core.Commands.Tests
{
    public class InfractCommand : BaseCommandModule
    {
        private readonly InfractionService _infractionService;
        private readonly IDatabaseService _dbService;

        public InfractCommand(IInfractionService service, IDatabaseService dbSerivice) => (_infractionService, _dbService) = ((InfractionService) service, dbSerivice);

        [Command]
        public async Task In(CommandContext ctx, DiscordMember member, TimeSpan duration, [RemainingText] string reason = "Not provided")
        {
            var infraction = await _infractionService.CreateTemporaryInfractionAsync(member, ctx.Member, InfractionType.Mute, reason, DateTime.Now.Add(duration));
            await _infractionService.MuteAsync(member, ctx.Channel, infraction);

            //await _infractionService.BanAsync(member, ctx.Channel, infraction);

            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromGuildEmote(ctx.Client, 795652577038565386));
        }
    }
}