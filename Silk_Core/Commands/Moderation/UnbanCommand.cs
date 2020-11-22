using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SilkBot.Extensions;
using SilkBot.Tools;
using SilkBot.Utilities;

namespace SilkBot.Commands.Moderation
{

    [Category(Categories.Mod)]
    public class UnbanCommand : BaseCommandModule
    {
        private readonly TimedEventService _eventService;
        public UnbanCommand(TimedEventService eventService) { _eventService = eventService; }

        [Command("unban")]
        [RequirePermissions(Permissions.BanMembers)]
        public async Task UnBan(CommandContext ctx, DiscordUser user, [RemainingText] string reason = "No reason given.")
        {
            if ((await ctx.Guild.GetBansAsync()).Select(b => b.User.Id).Contains(user.Id))
            {
                await user.UnbanAsync(ctx.Guild, reason);
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder(EmbedHelper.CreateEmbed(ctx, "", $"Unbanned {user.Username}#{user.Discriminator} `({user.Id})`! ")).AddField("Reason:", reason);
                var infraction = (TimedInfraction)_eventService.Events.FirstOrDefault(e => ((TimedInfraction)e).Id == user.Id);
                if (infraction is not null) _eventService.Events.TryRemove(infraction);
                await ctx.RespondAsync(embed: embed);
            }
            else
            {
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder(EmbedHelper.CreateEmbed(ctx, "", $"{user.Mention} is not banned!")).WithColor(new DiscordColor("#d11515"));
                await ctx.RespondAsync(embed: embed);
            }

        }
    }
}
