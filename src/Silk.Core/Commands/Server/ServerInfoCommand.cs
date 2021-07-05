using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Humanizer;
using Silk.Core.Data;
using Silk.Core.Utilities.HelpFormatter;
using Silk.Extensions;

namespace Silk.Core.Commands.Server
{
    [Category(Categories.Server)]
    public class ServerInfoCommand : BaseCommandModule
    {
        private readonly GuildContext _db;

        public ServerInfoCommand(GuildContext _db) => this._db = _db;

        [Command]
        [Description("Get info about the current Guild")]
        public async Task ServerInfo(CommandContext ctx)
        {
            DiscordGuild guild = await ctx.Client.GetGuildAsync(ctx.Guild.Id, true);
            
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithTitle($"Guild info for {guild.Name}:")
                .WithColor(DiscordColor.Gold)
                .WithFooter($"Silk! | Requested by: {ctx.User.Id}", ctx.Client.CurrentUser.AvatarUrl);

            embed.WithThumbnail(guild.IconUrl);

            embed.AddField("Server Icon:", guild.IconUrl is null ? "Not set." : $"[Link]({guild.IconUrl})", true)
                .AddField("Invite Splash:", guild.SplashUrl is null ? "Not set." : $"[Link]({guild.SplashUrl})", true)
                .AddField("Server banner:", guild.BannerUrl is null ? "Not set." : $"[Link]({guild.BannerUrl})", true);

            var stringBuilder = new StringBuilder();
            stringBuilder
                .AppendLine($"Max: {guild.MaxMembers}")
                .AppendLine($"Online: {guild.ApproximatePresenceCount.Value}")
                .AppendLine($"Offline: {guild.Members.Count - guild.ApproximatePresenceCount.Value}")
                .AppendLine($"**Total**: {guild.MemberCount}");
            embed.AddField("Members:", stringBuilder.ToString(), true);
            stringBuilder.Clear();


            var cTypes = guild.Channels.GroupBy(g => g.Value.Type);
            foreach (var type in cTypes)
            {
                _ = type.Key switch
                {
                    ChannelType.News => stringBuilder.AppendLine($"News: {type.Count()}"),
                    ChannelType.Text => stringBuilder.AppendLine($"Text: {type.Count()}"),
                    ChannelType.Voice => stringBuilder.AppendLine($"Voice: {type.Count()}"),
                    ChannelType.Stage => stringBuilder.AppendLine($"Stage: {type.Count()}"),
                    ChannelType.Category => stringBuilder.AppendLine($"Category: {type.Count()}"),
                    _ => stringBuilder.AppendLine($"Unknown ({type.Key}): {type.Count()}")
                };
            }
            stringBuilder.AppendLine($"**Total**: {guild.Channels.Count}/500");
            embed.AddField("Channels:", stringBuilder.ToString(), true);
            stringBuilder.Clear();

            var maxEmojis = guild.PremiumTier switch
            {
                PremiumTier.Tier_1 => 100,
                PremiumTier.Tier_2 => 150,
                PremiumTier.Tier_3 => 250,
                PremiumTier.None => 50
            };

            var tierName = guild.PremiumTier switch
            {
                PremiumTier.None => "(No level)",
                PremiumTier.Tier_1 => "(Level 1)",
                PremiumTier.Tier_2 => "(Level 2)",
                PremiumTier.Tier_3 => "(Level 3)"
            };

            stringBuilder
                .AppendLine($"Emojis: {ctx.Guild.Emojis.Count}/{maxEmojis * 2}")
                .AppendLine($"Roles: {guild.Roles.Count}/250")
                .AppendLine($"Boosts: {guild.PremiumSubscriptionCount ?? 0} {tierName}");

            embed.AddField("Other information:", stringBuilder.ToString(), true);
            stringBuilder.Clear();


            var creation = $"{DSharpPlus.Formatter.Timestamp(guild.CreationTimestamp - DateTime.UtcNow, TimestampFormat.LongDateTime)} ({DSharpPlus.Formatter.Timestamp(guild.CreationTimestamp - DateTime.UtcNow)})";
            embed.AddField("Server Owner:", guild.Owner.Mention, true)
                .AddField("Most recent member:", guild.Members.OrderBy(m => m.Value.JoinedAt).Last().Value.Mention, true)
                .AddField("Creation date:", creation);
                

            embed.AddField("Guild features:", guild.Features.Select(ft => ft.ToLower().Titleize()).Join(", "));

            await ctx.RespondAsync(embed);
        }
    }
}