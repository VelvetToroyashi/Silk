using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using SilkBot.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SilkBot.Commands.TestCommands
{
    public class ConfigCommand : BaseCommandModule
    {
        [Command("Config")]
        [HelpDescription("Set configuration here!")]
        public async Task TestLogChannelWorks(CommandContext ctx)
        {


            //Configuration
            var config = SilkBot.Bot.Instance.SilkDBContext.Guilds.AsQueryable().First(g => g.DiscordGuildId == ctx.Guild.Id);
            var embed = new DiscordEmbedBuilder()
                .WithAuthorExtension(ctx.Member.DisplayName, ctx.Member.AvatarUrl)
                .WithTitle("Current server config:")
                .WithColor(DiscordColor.Gold);
            var staffMembers = config.DiscordUserInfos
                .AsQueryable()
                .Where(member => member.Flags
                    .HasFlag(Models.UserFlag.Staff));
            embed.AddField("Staff members:", $"Number of staff: {staffMembers.Count()}, Top 10 members: {string.Join(", ", staffMembers.Take(10).Select(_ => $"<@!{_.UserId}>"))}");

            await ctx.RespondAsync(embed: embed);


            //var embed = new DiscordEmbedBuilder().WithAuthorExtension(ctx.Member.DisplayName, ctx.Member.AvatarUrl).WithTitle("Current server config:").WithColor(DiscordColor.Gold);
            ////Admins (might add <prefix>config add admin <roleid>)
            //var adminRoles = string.Join('\n', ctx.Guild.Roles.Where(role => role.Value.HasPermission(Permissions.Administrator) && !role.Value.IsManaged).OrderBy(n => n.Value.Name.Length).Select(r => r.Value.Mention));

            //if(adminRoles.Length < 1)
            //    adminRoles = "Could not find role with administrator permissions.";

            //embed.AddField($"Admin {(adminRoles.Split('\n').Count() > 1 ? "roles" : "role")}:", adminRoles, true);

            ////Moderator
            //var modRoles = string.Join('\n', ctx.Guild.Roles
            //    .Where(role => role.Value.HasPermission(Permissions.KickMembers) && 
            //    !role.Value.IsManaged && 
            //        !role.Value.HasPermission(Permissions.Administrator))
            //    .OrderBy(n => n.Value.Name.Length)
            //    .Select(r => r.Value.Mention));

            //if (modRoles.Length < 1)
            //    modRoles = "Could not find role with administrator permissions.";

            //embed.AddField($"Moderator {(modRoles.Split('\n').Count() > 1 ? "roles" : "role")}:", modRoles, true);


            //embed.AddField("    Muted role:", $"{(config.GuildInfo.MutedRole == 0 ? "Not set!" : $"<@&{config.GuildInfo.MutedRole}>")}", false);
            //embed.AddField("Logging channel:", $"{(config.GuildInfo.LoggingChannel == 0 ? "Not set!" : $"<#{config.GuildInfo.LoggingChannel}>")}", true);

            //embed.AddFooter(ctx);
            //await ctx.RespondAsync(embed: embed);

        }


        [Command("Config")]
        public async Task SetMute(CommandContext ctx, string mute, DiscordRole mutedRole)
        {
            var config = SilkBot.Bot.Instance.SilkDBContext.Guilds.AsQueryable().First(g => g.DiscordGuildId == ctx.Guild.Id);
            config.MuteRoleID = mutedRole.Id;
        }


        [Command("Config")]
        public async Task SetConfig(CommandContext ctx, string action, DiscordChannel channel)
        {
            switch (action.ToLower())
            {
                case "onmemberleave":
                case "onmemberjoin":
                case "onmemeberchange":
                case "greetingchannel":
                    await SetMemberChangeChannel(ctx, channel.Id);
                    break;
                case "generallog":
                case "generalloggingchannel":
                case "log_channel":
                case "loggging_channel":
                    //await SetLogs(ctx, channel.Id);
                    break;
                case "mod":
                    await SetModChannel(ctx, channel.Id);
                    break;
                default:
                    await ctx.RespondAsync("Sorry, but I can't tell what you're trying to setup.");
                    break;
            }
        }

        private async Task SetMemberChangeChannel(CommandContext ctx, ulong Id)
        {
            var config = SilkBot.Bot.Instance.SilkDBContext.Guilds.AsQueryable().First(g => g.DiscordGuildId == ctx.Guild.Id);
            config.LogMemberJoinOrLeave = true;
            config.MemberLeaveJoinChannel = Id;
            await SilkBot.Bot.Instance.SilkDBContext.SaveChangesAsync();

        }

        private async Task SetModChannel(CommandContext ctx, ulong Id)
        {
            var config = SilkBot.Bot.Instance.SilkDBContext.Guilds.AsQueryable().First(g => g.DiscordGuildId == ctx.Guild.Id);
            config.RoleChangeLogChannel = Id;
            config.MessageEditChannel = Id;
            await SilkBot.Bot.Instance.SilkDBContext.SaveChangesAsync();
        }

        private async Task SetLogs(CommandContext ctx, ulong id)
        {
            if (ctx.Guild.GetChannel(id) is null)
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
                //SilkBot.Bot.Instance.Data[ctx.Guild].GuildInfo.LoggingChannel = channelID;
                await ctx.RespondAsync(embed: EmbedHelper.CreateEmbed(ctx, $"Done! I'll log actions to {ctx.Guild.GetChannel(channelID).Mention}", DiscordColor.Gold));
            }
            else
            {
                //SilkBot.Bot.Instance.Data[ctx.Guild].GuildInfo.LoggingChannel = id;
                await ctx.RespondAsync(embed: EmbedHelper.CreateEmbed(ctx, $"Done! I'll log actions to {ctx.Guild.GetChannel(id).Mention}", DiscordColor.Gold));
            }
        }

        [Command("setmute")]
        public async Task SetMute(CommandContext ctx, DiscordRole mutedRole)
        {
            if (mutedRole is null)
            {
                var interactivity = ctx.Client.GetInteractivity();
                await ctx.RespondAsync("No role ID was passed in the config method. What role would you like for mutes?");
                var message = await interactivity.WaitForMessageAsync(msg => msg.Author == ctx.Message.Author &&
                ctx.Message.MentionedRoles.Count > 0,
                TimeSpan.FromSeconds(30));
                if (message.TimedOut)
                {
                    await ctx.RespondAsync("Setup timed out.");
                    return;
                }

                var (authName, authURL) = ctx.GetAuthor();
                var embed = new DiscordEmbedBuilder()
                    .WithAuthorExtension(authName, authURL)
                    .WithDescription($"Done! Muted role is set to {ctx.Guild.GetRole(message.Result.MentionedRoles.First().Id).Mention}")
                    .WithColor(DiscordColor.Gold)
                    .AddFooter(ctx);
                //SilkBot.Bot.Instance.Data[ctx.Guild].GuildInfo.MutedRole = role;
                await ctx.RespondAsync(embed: embed);
            }
            else
            {
                if (mutedRole is null)
                {
                    await ctx.RespondAsync("That isn't a role!");
                    return;
                }
                var (authName, authURL) = ctx.GetAuthor();
                var embed = new DiscordEmbedBuilder()
                    .WithAuthorExtension(authName, authURL)
                    .WithDescription($"Done! Muted role is set to {mutedRole.Mention}")
                    .WithColor(DiscordColor.Gold)
                    .AddFooter(ctx);
                //SilkBot.Bot.Instance.Data[ctx.Guild].GuildInfo.MutedRole = roleID;
                await ctx.RespondAsync(embed: embed);
            }
        }
    }
}
