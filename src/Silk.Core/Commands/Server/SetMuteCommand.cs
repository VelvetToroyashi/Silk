using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MediatR;
using Silk.Core.Utilities;
using Silk.Data.MediatR;
using Silk.Data.Models;
using Silk.Extensions.DSharpPlus;

namespace Silk.Core.Commands.Server
{
    public class SetMuteCommand : BaseCommandModule
    {
        private readonly IMediator _mediator;
        public SetMuteCommand(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Command]
        [RequireGuild]
        [RequireFlag(UserFlag.Staff)]
        [Description("Set the mute role to use when calling mute command, or auto-mod")]
        public async Task SetMute(CommandContext ctx, DiscordRole role)
        {
            if ((role.Permissions & Permissions.SendMessages) is not 0) // Does not have permission
            {
                await ctx.RespondAsync("Role isn't restrictive!");
                return;
            }
            var builder = new DiscordMessageBuilder();
            builder.WithReply(ctx.Message.Id);
            builder.WithoutMentions();

            builder.WithContent($"Alright! I'll use {role.Mention} for muting!");
            await ctx.RespondAsync(builder);
            await _mediator.Send(new GuildConfigRequest.Update {GuildId = ctx.Guild.Id, MuteRoleId = role.Id});
            
        }
        
    }
}