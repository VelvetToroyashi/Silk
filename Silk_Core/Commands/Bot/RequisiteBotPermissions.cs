using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SilkBot.Extensions;
using SilkBot.Utilities;
using System.Text;
using System.Threading.Tasks;

namespace SilkBot.Commands.Bot
{
    public class RequisiteBotPermissions : BaseCommandModule
    {
        [Command("Perms")]
        [HelpDescription("The bot's needed permissions, and what commands they affect.")]
        public async Task GetRequiredPermissions(CommandContext ctx)
        {
            string prefix = ctx.Prefix;

            var embed = EmbedHelper.CreateEmbed(ctx, "Permissions:", DiscordColor.CornflowerBlue);
            var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);

            var manageMessage = bot.HasPermission(Permissions.ManageMessages);
            var kick = bot.HasPermission(Permissions.KickMembers);
            var ban = bot.HasPermission(Permissions.BanMembers);
            var manageRoles = bot.HasPermission(Permissions.ManageRoles);

            var sb = new StringBuilder();

            sb.AppendLine($"`Manage Messages`: {GetStatusEmoji(manageMessage)}\nAffected commands: `{prefix}clear`, `{prefix}clean`; __error messages will persist if false.__\n");
            sb.AppendLine($"`Manage Roles`: {GetStatusEmoji(manageRoles)}\nAffected commands: `{prefix}role`\n");
            sb.AppendLine($"`Kick Members` {GetStatusEmoji(kick)}\nAffected commands: `{prefix}kick`\n");
            sb.AppendLine($"`Ban Members` {GetStatusEmoji(ban)}\nAffected commands: `{prefix}ban`\n");

            embed.WithTitle("Permissions:");
            embed.WithDescription(sb.ToString());

            await ctx.RespondAsync(embed: embed);
        }

        private static string GetStatusEmoji(bool requirementMet) => requirementMet ? ":white_check_mark:" : ":x:";
    }
}