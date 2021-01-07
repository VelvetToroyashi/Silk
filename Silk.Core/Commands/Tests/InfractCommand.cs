using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DSharpPlus;
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

        public InfractCommand(IInfractionService service, IDatabaseService dbSerivice) =>
            (_infractionService, _dbService) = ((InfractionService) service, dbSerivice);

        [Command]
        public async Task In(CommandContext ctx, DiscordMember member)
        {
            _infractionService.AddInfraction(ctx.Member, new()
            {
                Enforcer = ctx.User.Id,
                GuildId = ctx.Guild.Id,
                InfractionTime = DateTime.Now,
                InfractionType = InfractionType.Mute,
                User = await _dbService.GetOrAddUserAsync(ctx.Guild.Id, member.Id),
                Reason = "Infraction Test",
                UserId = member.Id
            });
            
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromGuildEmote(ctx.Client, 795652577038565386));
        }
    }
}