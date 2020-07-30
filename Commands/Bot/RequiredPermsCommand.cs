using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilkBot.Commands.Bot
{
    public class RequiredPermsCommand : BaseCommandModule
    {
        [Command("Perms")]
        public async Task GetRequiredPermissions(CommandContext ctx)
        {
            var permissions = new Dictionary<string, bool>();
            var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            var role = bot.Roles.LastOrDefault().Permissions;

            permissions.Add("Administrator", role.HasPermission(Permissions.Administrator));
            permissions.Add("Kick", role.HasPermission(Permissions.KickMembers));
            permissions.Add("Ban", role.HasPermission(Permissions.BanMembers));
            permissions.Add("Manage", role.HasPermission(Permissions.ManageMessages));


            var permStrings = new List<string>();
            var no = DiscordEmoji.FromName(ctx.Client, ":x:");
            var yes = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");

            permStrings.Add($"Admin (Provides an override for all permissions listed below.? {(permissions["Administrator"] ? yes : no)}");
            var kickk = permissions["Administrator"] ? yes : (permissions["Kick"] ? yes : no);
            permStrings.Add($"Kick members? {kickk} {(permissions["Kick"] ? "" : "`!kick will not work!`")}");
            permStrings.Add($"Ban members? {(permissions["Administrator"] ? yes : (permissions["Ban"] ? yes : no))}");
            permStrings.Add($"Manage messages (Used for clean and clear)? {(permissions["Administrator"] ? yes : (permissions["Manage"] ? yes : no))}");

            var eb = new DiscordEmbedBuilder()
                .WithTitle("Am I (able to):")
                .WithDescription($"{permStrings[0]} \n {permStrings[1]} \n {permStrings[2]} \n {permStrings[3]} \n")
                .WithColor(DiscordColor.CornflowerBlue)
                .WithAuthor(ctx.Member.DisplayName, iconUrl: ctx.User.AvatarUrl);

            await ctx.RespondAsync(embed: eb);
        }
    }
}
