using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace SilkBot
{
    public class UnbanCommand : BaseCommandModule
    {
        [Command("unban")]
        [RequirePermissions(Permissions.BanMembers)]
        public async Task UnBan(CommandContext ctx, DiscordUser user, [RemainingText] string reason = "No reason given.")
        {
            if ((await ctx.Guild.GetBansAsync()).Any(ban => ban.User.Id == user.Id))
            {
                await user.UnbanAsync(ctx.Guild, reason);
                var embed = new DiscordEmbedBuilder(EmbedHelper.CreateEmbed(ctx, "", $"Unbanned {user.Username}#{user.Discriminator} `({user.Id})`! ")).AddField("Reason:", reason);

                await ctx.RespondAsync(embed: embed);
            }
            else
            {
                var embed = new DiscordEmbedBuilder(EmbedHelper.CreateEmbed(ctx, "", $"{user.Mention} is not banned!")).WithColor(new DiscordColor("#d11515"));
                await ctx.RespondAsync(embed: embed);
            }
        }
    }
}