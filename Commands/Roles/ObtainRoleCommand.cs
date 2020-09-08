using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using SilkBot.Utilities;

namespace SilkBot.Commands.Roles
{
    public class ObtainRoleCommand : BaseCommandModule
    {

        [Command("Role")]
        [HelpDescription("Grab a role!", "[p]role <rolename>")]
        public async Task ObtainRole(CommandContext ctx, [RemainingText] string roles)
        {
            if (roles is null) return;
            var _roles = roles.Split(',');
            var guild = SilkBot.Bot.Instance.SilkDBContext.Guilds.AsQueryable().First(g => g.DiscordGuildId == ctx.Guild.Id);
            //If a config exists, use that, else assume no config exists and throw an error.//
            foreach (var role in _roles)
            {
                var parsedRole = ctx.Guild.Roles.First(r => r.Value.Name.ToLower() == role.ToLower()).Value;

                if (guild.SelfAssignableRoles.Count > 0)
                {

                    var selfAssignableRoles = guild.SelfAssignableRoles;

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
                                .WithDescription($"Sorry, but this server has not set up self-assignable roles.")
                                .WithFooter("Silk", ctx.Client.CurrentUser.AvatarUrl)
                                .WithTimestamp(DateTime.Now));
                }

            }

        }
    }
}
