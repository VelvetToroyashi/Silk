using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MediatR;
using Silk.Core.Data.MediatR.Unified.Guilds;
using Silk.Core.Data.Models;
using Silk.Core.Discord.Utilities;
using Silk.Core.Discord.Utilities.HelpFormatter;

namespace Silk.Core.Discord.Commands.Server.Roles
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

            GuildConfig config = await _mediator.Send(new GetGuildConfigRequest(ctx.Guild.Id));

            if (roles.Any())
            {
                var botPos = ctx.Guild.CurrentMember.Roles.Last()!.Position;
                var unavailableRoles = roles.Where(r => r.Position > botPos);

                if (unavailableRoles.Any())
                {
                    builder.WithContent($"I can't give out these roles: {string.Join(", ", unavailableRoles.Select(r => r.Mention))}");
                    await ctx.RespondAsync(builder);
                    roles = roles.Except(unavailableRoles).ToArray();
                }


                await _mediator.Send(new UpdateGuildConfigRequest(config.GuildId)
                {
                    SelfAssignableRoles = roles.Select(r => new SelfAssignableRole {Id = r.Id}).ToList()
                });
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("👍"));
            }

            var embedBuilder = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Gold)
                .WithTitle("Currently assignable roles")
                .WithDescription(string.Join('\n', config.SelfAssignableRoles.Select(r => $"<@&{r.Id}>")));
            await ctx.RespondAsync(embedBuilder);
        }

        [Command("role")]
        [RequireGuild]
        [Description("Get a role!")]
        public async Task Role(CommandContext ctx, [RemainingText] params DiscordRole[] roles)
        {
            GuildConfig config = await _mediator.Send(new GetGuildConfigRequest(ctx.Guild.Id));
            if (!roles.Any())
            {
                var embedBuilder = new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Gold)
                    .WithTitle("Currently assignable roles")
                    .WithDescription(string.Join('\n', config.SelfAssignableRoles.Select(r => $"<@&{r.Id}>")));
                await ctx.RespondAsync(embedBuilder);
            }
            else
            {
                foreach (DiscordRole role in roles)
                {
                    if (!config.SelfAssignableRoles.Select(r => r.Id).Contains(role.Id))
                    {
                        await ctx.RespondAsync("Sorry, but that's not an assignable role! Contact your server's staff for assistance!");
                    }
                    else
                    {
                        if (ctx.Member.Roles.Contains(role))
                            await ctx.Member.RevokeRoleAsync(role);
                        else await ctx.Member.GrantRoleAsync(role);
                    }
                }
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("👍"));
            }
        }
    }
}