using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using SilkBot.ServerConfigurations;
using SilkBot.Utilities;
using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace SilkBot.Commands.TestCommands
{
    public class ConfigCommand : BaseCommandModule
    {
        [Command("Config")]
        [HelpDescription("Something wrong with your settings? Run this command to verify your configuration is set properly!")]
        public async Task TestLogChannelWorks(CommandContext ctx)
        {


                //Configuration
                var config = SilkBot.Bot.Instance.Data[ctx.Guild];
                var embed = new DiscordEmbedBuilder().WithAuthorExtension(ctx.Member.DisplayName, ctx.Member.AvatarUrl).WithTitle("Current server config:").WithColor(DiscordColor.Gold);
                //Admins (might add <prefix>config add admin <roleid>)
                var adminRoles = string.Join('\n', ctx.Guild.Roles.Where(role => role.Value.HasPermission(Permissions.Administrator) && !role.Value.IsManaged).OrderBy(n => n.Value.Name.Length).Select(r => r.Value.Mention));
                
                if(adminRoles.Length < 1)
                    adminRoles = "Could not find role with administrator permissions.";
                
                embed.AddField($"Admin {(adminRoles.Split('\n').Count() > 1 ? "roles" : "role")}:", adminRoles, true);
                
                //Moderator
                var modRoles = string.Join('\n', ctx.Guild.Roles
                    .Where(role => role.Value.HasPermission(Permissions.KickMembers) && 
                    !role.Value.IsManaged && 
                        !role.Value.HasPermission(Permissions.Administrator))
                    .OrderBy(n => n.Value.Name.Length)
                    .Select(r => r.Value.Mention));

                if (modRoles.Length < 1)
                    modRoles = "Could not find role with administrator permissions.";
                
                embed.AddField($"Moderator {(modRoles.Split('\n').Count() > 1 ? "roles" : "role")}:", modRoles, true);


                embed.AddField("    Muted role:", $"{(config.GuildInfo.MutedRole == 0 ? "Not set!" : $"<@&{config.GuildInfo.MutedRole}>")}", false);
                embed.AddField("Logging channel:", $"{(config.GuildInfo.LoggingChannel == 0 ? "Not set!" : $"<#{config.GuildInfo.LoggingChannel}>")}", true);

                embed.AddFooter(ctx);
                await ctx.RespondAsync(embed: embed);
            
        }
        [Command("Config")]
        public Task SetConfig(CommandContext ctx, string action, ulong Id = 0) =>
            action.ToLowerInvariant() switch
            {
                "set_mute"      => SetMute(ctx, Id),
                "setmute"       => SetMute(ctx, Id),
                "mute"          => SetMute(ctx, Id),

                "log"           => SetLogs(ctx, Id),
                "log_to"        => SetLogs(ctx, Id),
                "set_log"       => SetLogs(ctx, Id),
                "logchannel"    => SetLogs(ctx, Id),
                "log_channel"   => SetLogs(ctx, Id),

                _               => Task.CompletedTask,
            };

        private async Task SetLogs(CommandContext ctx, ulong id)
        {
            if(ctx.Guild.GetChannel(id) is null)
            {
                await ctx.RespondAsync("No channel ID was passed in config command. What channel do you want me to log to?");
                var interactivity = ctx.Client.GetInteractivity();
                var message = await interactivity.WaitForMessageAsync(msg => 
                msg.Author == ctx.Message.Author && 
                ulong.TryParse(msg.Content, out var channelID) && 
                channelID != 0 &&
                ctx.Guild.GetChannel(channelID) != null, 
                TimeSpan.FromSeconds(30));
                if (message.TimedOut)
                {
                    await ctx.RespondAsync("Setup timed out.");
                    return;
                }
                var channelID = ulong.Parse(message.Result.Content);
                SilkBot.Bot.Instance.Data[ctx.Guild].GuildInfo.LoggingChannel = channelID;
                await ctx.RespondAsync(embed: EmbedHelper.CreateEmbed(ctx, $"Done! I'll log actions to {ctx.Guild.GetChannel(channelID).Mention}", DiscordColor.Gold));
            }
            else
            {
                SilkBot.Bot.Instance.Data[ctx.Guild].GuildInfo.LoggingChannel = id;
                await ctx.RespondAsync(embed: EmbedHelper.CreateEmbed(ctx, $"Done! I'll log actions to {ctx.Guild.GetChannel(id).Mention}", DiscordColor.Gold));
            }
        }

        private async Task SetMute(CommandContext ctx, ulong roleID)
        {
            if(roleID == 0)
            {
                var interactivity = ctx.Client.GetInteractivity();
                await ctx.RespondAsync("No role ID was passed in the config method. What role would you like for mutes?");
                var message = await interactivity.WaitForMessageAsync(msg => msg.Author == ctx.Message.Author && 
                ulong.TryParse(msg.Content, out var roleId) &&
                ctx.Guild.GetRole(roleId) != null, 
                TimeSpan.FromSeconds(30));
                if (message.TimedOut)
                {
                    await ctx.RespondAsync("Setup timed out.");
                    return;
                }
                var role = ulong.Parse(message.Result.Content);
                var (authName, authURL) = ctx.GetAuthor();
                var embed = new DiscordEmbedBuilder()
                    .WithAuthorExtension(authName, authURL)
                    .WithDescription($"Done! Muted role is set to {ctx.Guild.GetRole(role).Mention}")
                    .WithColor(DiscordColor.Gold)
                    .AddFooter(ctx);
                SilkBot.Bot.Instance.Data[ctx.Guild].GuildInfo.MutedRole = role;
                await ctx.RespondAsync(embed: embed);
            }
            else
            {
                if(ctx.Guild.GetRole(roleID) is null)
                {
                    await ctx.RespondAsync("That isn't a role!");
                    return;
                }
                var (authName, authURL) = ctx.GetAuthor();
                var embed = new DiscordEmbedBuilder()
                    .WithAuthorExtension(authName, authURL)
                    .WithDescription($"Done! Muted role is set to {ctx.Guild.GetRole(roleID).Mention}")
                    .WithColor(DiscordColor.Gold)
                    .AddFooter(ctx);
                SilkBot.Bot.Instance.Data[ctx.Guild].GuildInfo.MutedRole = roleID;
                await ctx.RespondAsync(embed: embed);
            }
        }
    }
}
