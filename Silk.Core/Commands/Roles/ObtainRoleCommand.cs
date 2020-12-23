using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Core.Database.Models;
using Silk.Core.Utilities;

namespace Silk.Core.Commands.Roles
{
    [Category(Categories.Misc)]
    public class ObtainRoleCommand : BaseCommandModule
    {
        [Command("Role")]
        [Description("Grab a role!")]
        public async Task ObtainRole(CommandContext ctx, [RemainingText] string? roles)
        {
            if (roles is null) return;
            string[] _roles = roles.Split(',');
            GuildModel guild = Core.Bot.Instance!.SilkDBContext.Guilds.First(g => g.Id == ctx.Guild.Id);
            foreach (string role in _roles)
            {
                DiscordRole parsedRole = ctx.Guild.Roles.First(r => r.Value.Name.ToLower() == role.ToLower()).Value;

                if (guild.Configuration.SelfAssignableRoles.Count > 0)
                {
                    List<SelfAssignableRole> selfAssignableRoles = guild.Configuration.SelfAssignableRoles;

                    if (selfAssignableRoles.Any(saRole => saRole.RoleId == parsedRole.Id))
                    {
                        if (!ctx.Member.Roles.Any(r => r == parsedRole))
                        {
                            await ctx.Member.GrantRoleAsync(parsedRole);
                            await ctx.RespondAsync(embed:
                                new DiscordEmbedBuilder()
                                    .WithAuthor(ctx.Member.DisplayName, iconUrl: ctx.Member.AvatarUrl)
                                    .WithColor(DiscordColor.CornflowerBlue)
                                    .WithDescription($"Gave you the role {parsedRole.Mention}")
                                    .WithFooter("Silk", ctx.Client.CurrentUser.AvatarUrl)
                                    .WithTimestamp(DateTime.Now)
                            );
                        }
                        else
                        {
                            await ctx.Member.RevokeRoleAsync(parsedRole);
                            await ctx.RespondAsync(embed:
                                new DiscordEmbedBuilder()
                                    .WithAuthor(ctx.Member.DisplayName, iconUrl: ctx.Member.AvatarUrl)
                                    .WithColor(DiscordColor.CornflowerBlue)
                                    .WithDescription($"Revoked {parsedRole.Mention}")
                                    .WithFooter("Silk", ctx.Client.CurrentUser.AvatarUrl)
                                    .WithTimestamp(DateTime.Now));
                        }
                    }
                    else
                    {
                        await ctx.RespondAsync(embed:
                            new DiscordEmbedBuilder()
                                .WithAuthor(ctx.Member.DisplayName, iconUrl: ctx.Member.AvatarUrl)
                                .WithColor(DiscordColor.IndianRed)
                                .WithDescription($"Sorry, but {parsedRole.Mention} is NOT available to assign.")
                                .WithFooter("Silk", ctx.Client.CurrentUser.AvatarUrl)
                                .WithTimestamp(DateTime.Now));
                    }
                }
                else
                {
                    await ctx.RespondAsync(embed:
                        new DiscordEmbedBuilder()
                            .WithAuthor(ctx.Member.DisplayName, iconUrl: ctx.Member.AvatarUrl)
                            .WithColor(DiscordColor.IndianRed)
                            .WithDescription("Sorry, but this server has not set up self-assignable roles.")
                            .WithFooter("Silk", ctx.Client.CurrentUser.AvatarUrl)
                            .WithTimestamp(DateTime.Now));
                }
            }
        }
    }
}