using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace SilkBot
{

    public class UnbanCommand : BaseCommandModule
    { 
        [Command("unban")]
        [RequirePermissions(Permissions.BanMembers)]
        public async Task UnBan(CommandContext ctx, DiscordUser user, [RemainingText] string reason = null)
        {
            await ctx.Guild.UnbanMemberAsync(user, reason);
            var embed = EmbedGenerator.CreateEmbed(ctx, "", $"Unanned {user.Username}{user.Discriminator}! Reason: {(reason is null ? "No reason given." : reason)}");

            await ctx.RespondAsync(embed: embed);
        }
    }
}
