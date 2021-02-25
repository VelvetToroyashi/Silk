using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Utilities.HelpFormatter;
using Silk.Data;
using Silk.Data.Models;
using Silk.Extensions;

namespace Silk.Core.Commands.Server
{
    [Category(Categories.Server)]
    public class ServerInfoCommand : BaseCommandModule
    {
        private readonly IDbContextFactory<SilkDbContext> _dbFactory;

        public ServerInfoCommand(IDbContextFactory<SilkDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        [Command]
        [Description("Get info about the current Guild")]
        public async Task ServerInfo(CommandContext ctx)
        {
            DiscordGuild guild = ctx.Guild;
            SilkDbContext db = _dbFactory.CreateDbContext();

            var staffMembers = db.Guilds.Include(g => g.Users)
                .First(g => g.Id == guild.Id)
                .Users
                .Where(u => u.Flags.HasFlag(UserFlag.Staff));

            int staffCount = staffMembers.Count();

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithTitle($"Guild info for {guild.Name}:")
                .WithColor(DiscordColor.Gold)
                .WithFooter($"Silk! | Requested by: {ctx.User.Id}", ctx.Client.CurrentUser.AvatarUrl);

            embed.WithThumbnail(guild.IconUrl);

            if (guild.PremiumSubscriptionCount.Value > 0)
                embed.AddField("Boosts:", $"{guild.PremiumSubscriptionCount.Value} boosts (level {guild.PremiumTier})");

            if (guild.Features.Count > 0)
                embed.AddField("Enabled guild features: ", guild.Features.Select(f => f.Humanize(LetterCasing.Title)).Join(", "));

            embed.AddField("Verification Level:", guild.VerificationLevel.ToString().ToUpper());
            embed.AddField("Member Count:", guild.MemberCount.ToString());
            embed.AddField("Owner:", guild.Owner.Mention);
            embed.AddField("Approximate staff member count:", staffCount.ToString());

            await ctx.RespondAsync(embed);
        }
    }
}