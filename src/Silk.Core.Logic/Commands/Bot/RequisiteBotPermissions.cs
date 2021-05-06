using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Core.Discord.Utilities.HelpFormatter;
using Silk.Extensions;

namespace Silk.Core.Logic.Commands.Bot
{
    [Category(Categories.Bot)]
    public class RequisiteBotPermissions : BaseCommandModule
    {
        [Command("perms")]
        [Description("The bot's needed permissions, and what commands they affect.")]
        public async Task GetRequiredPermissions(CommandContext ctx)
        {
            string prefix = ctx.Prefix;

            var description = ctx.Command.Description;

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.CornflowerBlue)
                .WithTitle("Silk's Permissions:")
                .WithDescription(description + "\n");

            DiscordMember bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);

            bool manageMessage = bot.HasPermission(Permissions.ManageMessages);
            bool kick = bot.HasPermission(Permissions.KickMembers);
            bool ban = bot.HasPermission(Permissions.BanMembers);
            bool manageRoles = bot.HasPermission(Permissions.ManageRoles);

            // Todo: Match / Collect commands with permissions the bot needs (could be based on Attribute), so the command names below can be updated dynamically

            embed.AddField($"`Manage Messages` {GetStatusEmoji(manageMessage)}\n", $"Affected commands: `{prefix} clear`, " + $"`{prefix} clean`; __error messages will persist if false.__\n");
            embed.AddField($"`Manage Roles` {GetStatusEmoji(manageRoles)}\n", $"Affected commands: `{prefix} role-info`\n");
            embed.AddField($"`Kick Members` {GetStatusEmoji(kick)}\n", $"Affected commands: `{prefix} kick`\n");
            embed.AddField($"`Ban Members` {GetStatusEmoji(ban)}\n", $"Affected commands: `{prefix} ban`\n");

            await ctx.RespondAsync(embed);
        }

        private static string GetStatusEmoji(bool requirementMet)
        {
            return requirementMet ? ":white_check_mark:" : ":x:";
        }
    }
}