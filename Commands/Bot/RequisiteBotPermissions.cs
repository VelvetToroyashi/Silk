using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SilkBot.Utilities;
using System.Linq;
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

            var prefix = SilkBot.Bot.Instance.SilkDBContext.Guilds.Where(guild => guild.DiscordGuildId == ctx.Guild.Id).AsEnumerable().Select(_ => _.Prefix);
            var embed = EmbedHelper.CreateEmbed(ctx, "Permissions:", DiscordColor.CornflowerBlue);
            var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);

            var manageMessage = bot.HasPermission(Permissions.ManageMessages);
            var kick = bot.HasPermission(Permissions.KickMembers);
            var ban = bot.HasPermission(Permissions.BanMembers);
            var manageRoles = bot.HasPermission(Permissions.ManageRoles);

            var sb = new StringBuilder();

            sb.AppendLine($"`Manage Messages`: {(manageMessage ? ":white_check_mark:" : ":x:")}\nAffected commands: `{prefix}clear`, `{prefix}clean`; __error messages will persist if false.__");
            sb.AppendLine($"`Manage Roles`: {(manageRoles ? ":white_check_mark:" : ":x:")}\nAffected commands: {prefix}role");
            sb.AppendLine($"`Kick Members` {(kick ? ":white_check_mark:" : ":x:")}\nAffected commands: {prefix}kick");
            sb.AppendLine($"`Ban Members` {(ban ? ":white_check_mark:" : ":x:")}\nAffected commands: {prefix}ban");

            embed.WithTitle("Permissions:");
            embed.WithDescription(sb.ToString());



            await ctx.RespondAsync(embed: embed);
        }
    }
}
