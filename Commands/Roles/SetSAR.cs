using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SilkBot.Exceptions;
using SilkBot.ServerConfigurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilkBot.Commands.Roles
{
    public class SetSAR : BaseCommandModule
    {
        [Command("Assign")]
        [Aliases("sar", "selfassignablerole", "selfrole")]
        [HelpDescription("Allows you to set self assignable roles. Role menu coming soon:tm:. All Self-Assignable Roles are opt-*in*.")]
        public async Task SetSelfAssignableRole(CommandContext ctx, params DiscordRole[] roles)
        {
            if (roles.Count() < 1)
            {
                await ctx.RespondAsync("Roles canont be empty!");
                return;
            }
            if (!ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.KickMembers))
            {
                throw new InsufficientPermissionsException();
            }

            var serverconfig = ServerConfigurationManager.LocalConfiguration.Values.Any(val => val.Guild == ctx.Guild.Id) ? ServerConfigurationManager.LocalConfiguration[ctx.Guild.Id] : null;
            if (serverconfig is null)
            {

                    var config = await ServerConfigurationManager.Instance.GenerateConfigurationFromIdAsync(ctx.Guild.Id);
                    ServerConfigurationManager.LocalConfiguration.TryAdd(ctx.Guild.Id, config);
                    var ebStringBuilder = new StringBuilder("I added/removed ");
                    foreach (var role in roles)
                    {
                        if (!config.SelfAssignableRoles.Contains(role.Id))
                            config.SelfAssignableRoles.Add(role.Id);
                        else
                            config.SelfAssignableRoles.Remove(role.Id);

                        ebStringBuilder.Append($" {ctx.Guild.GetRole(role.Id).Mention}");
                    }
                    ebStringBuilder.Append("!");

                    await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
                        .WithAuthor(ctx.Member.DisplayName, iconUrl: ctx.Member.AvatarUrl)
                        .WithDescription(ebStringBuilder.ToString())
                        .WithFooter("Silk", ctx.Client.CurrentUser.AvatarUrl)
                        .WithTimestamp(DateTime.Now));

            }
            else
            {
                var config = ServerConfigurationManager.LocalConfiguration[ctx.Guild.Id];
                var addedList = new List<string>();
                var removedList = new List<string>();
                if (config.SelfAssignableRoles is null)
                    config.SelfAssignableRoles = new List<ulong>();
                var ebStringBuilder = new StringBuilder("Added Roles: ");
                foreach (var role in roles)
                {
                    if (!config.SelfAssignableRoles.Contains(role.Id)) 
                    {
                        config.SelfAssignableRoles.Add(role.Id);
                        addedList.Add(role.Mention);
                    }

                    else
                    {
                        config.SelfAssignableRoles.Remove(role.Id);
                        removedList.Add(role.Mention);
                    }
                }

                if (addedList.Any())
                    foreach (var addedRole in addedList)
                        ebStringBuilder.Append(addedRole);
                else
                    ebStringBuilder.Append("none");
                
                ebStringBuilder.AppendLine();
                ebStringBuilder.AppendLine("Removed Roles: " + (removedList.Any() ? "" : "none"));

                foreach(var removedRole in removedList)
                {
                    ebStringBuilder.Append(removedRole);
                }
                
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
                    .WithAuthor(ctx.Member.DisplayName, iconUrl: ctx.Member.AvatarUrl)
                    .WithDescription(ebStringBuilder.ToString())
                    .WithFooter("Silk", ctx.Client.CurrentUser.AvatarUrl)
                    .WithTimestamp(DateTime.Now));

            }
        }
    }
}
