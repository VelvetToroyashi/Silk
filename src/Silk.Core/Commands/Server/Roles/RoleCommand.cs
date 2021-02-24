using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MediatR;
using Silk.Core.Utilities;
using Silk.Core.Utilities.HelpFormatter;
using Silk.Data.MediatR;
using Silk.Data.Models;

namespace Silk.Core.Commands.Server.Roles
{
    [Category(Categories.Server)]
    public class RoleCommand : BaseCommandModule
    {
        private readonly IMediator _mediator;
        public RoleCommand(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Command]
        [RequireGuild]
        [RequireFlag(UserFlag.Staff)]
        [Description("Toggle roles to be self-assignable on this server!")]
        public async Task SelfRole(CommandContext ctx, [RemainingText] params DiscordRole[] roles)
        {
            var builder = new DiscordMessageBuilder();
            builder.WithReply(ctx.Message.Id);

            GuildConfig config = await _mediator.Send(new GuildConfigRequest.Get(ctx.Guild.Id));
            
            var botPos = ctx.Guild.CurrentMember.Roles.Last().Position;
            var unavailableRoles = roles.Where(r => r.Position > botPos);
            
            if (unavailableRoles.Any())
            {
                builder.WithContent($"I can't give out these roles: {string.Join(", ", unavailableRoles.Select(r => r.Mention))}");
                await ctx.RespondAsync(builder);
                roles = roles.Except(unavailableRoles).ToArray();
            }

            foreach (var r in roles)
            {
                var ro = config.SelfAssignableRoles.SingleOrDefault(s => s.Id == r.Id);
                
                if (ro is not null)
                {
                    config.SelfAssignableRoles.Remove(ro);
                }
                else
                {
                    config.SelfAssignableRoles.Add(new() {Id = r.Id});
                }
            }
            await _mediator.Send(new GuildConfigRequest.Update {GuildId = config.GuildId, SelfAssignableRoles = config.SelfAssignableRoles});
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("👍"));

            var embedBuilder = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Gold)
                .WithTitle("Currently assignable roles")
                .WithDescription(string.Join('\n', config.SelfAssignableRoles.Select(r => $"<@&{r.Id}>")));
            await ctx.RespondAsync(embedBuilder);

        }
        
        [Command]
        [RequireGuild]
        [Description("Get a role!")]
        public async Task Role(CommandContext ctx, DiscordRole role)
        {
            GuildConfig config = await _mediator.Send(new GuildConfigRequest.Get(ctx.Guild.Id));

            if (!config.SelfAssignableRoles.Select(r => r.Id).Contains(role.Id))
            {
                await ctx.RespondAsync("Sorry, but that's not an assignable role! Contact your server's staff for assistance!");
                return;
            }
            
            if (ctx.Member.Roles.Contains(role))
            {
                await ctx.Member.RevokeRoleAsync(role);
            }
            else
            {
                await ctx.Member.GrantRoleAsync(role);
            }
            
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("👍"));
        }
    }
}