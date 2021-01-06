using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Core.Database.Models;
using Silk.Core.Tools;
using Silk.Core.Utilities;
using Silk.Extensions;

namespace Silk.Core.Commands.Moderation
{
    [Category(Categories.Mod)]
    public class UnbanCommand : BaseCommandModule
    {
        private readonly TimedEventService _eventService;

        public UnbanCommand(TimedEventService eventService)
        {
            _eventService = eventService;
        }

        [Command("unban")]
        [RequireFlag(UserFlag.Staff)]
        public async Task UnBan(CommandContext ctx, DiscordUser user, [RemainingText] string reason = "No reason given.")
        {
            if (!ctx.Member.HasPermission(Permissions.BanMembers))
            {
                await ctx.RespondAsync("[You] Can't ban, can't unban. Sorry.").ConfigureAwait(false);
                return;
            }
            
            if ((await ctx.Guild.GetBansAsync()).Select(b => b.User.Id).Contains(user.Id))
            {
                await user.UnbanAsync(ctx.Guild, reason);
                DiscordEmbedBuilder embed =
                    new DiscordEmbedBuilder(EmbedHelper.CreateEmbed(ctx, "",
                        $"Unbanned {user.Username}#{user.Discriminator} `({user.Id})`! ")).AddField("Reason:", reason);
                var infraction =
                    (TimedInfraction) _eventService.Events.FirstOrDefault(e => ((TimedInfraction) e).Id == user.Id);
                if (infraction is not null) _eventService.Events.TryRemove(infraction);
                await ctx.RespondAsync(embed: embed);
            }
            else
            {
                DiscordEmbedBuilder embed =
                    new DiscordEmbedBuilder(EmbedHelper.CreateEmbed(ctx, "", $"{user.Mention} is not banned!"))
                        .WithColor(new DiscordColor("#d11515"));
                await ctx.RespondAsync(embed: embed);
            }
        }
    }
}