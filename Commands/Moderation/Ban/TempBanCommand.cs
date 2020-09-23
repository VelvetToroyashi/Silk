using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SilkBot.Tools;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using SilkBot.Models;

namespace SilkBot.Commands.Moderation.Ban
{
    public class TempBanCommand : BaseCommandModule
    {
        private const string defaultFormat = "$mention was $action from the guild for $duration $reason";
        [Command("tempban")]
        public async Task TempBan(CommandContext ctx, DiscordMember user, string duration, [RemainingText] string reason = "Not provided.")
        {
            using var db = new SilkDbContext();
            var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            var now = DateTime.Now;
            var banDuration = GetTimeFromInput(duration);
            var banFailed = CanBan(bot, ctx.Member, user);
            var embed = GenerateBaseEmbed(ctx.User, ctx.Client.CurrentUser.AvatarUrl, now);

            if (banFailed is null)
            {
                embed.WithDescription(banFailed.FailureReason.Replace("$user", user.Mention));
                embed.WithColor(DiscordColor.Red);
                await ctx.RespondAsync(embed: embed);
                return;
            }
            else
            {
                await user.SendMessageAsync($"You have been temporarily banned from `{ctx.Guild.Name}` for `{banDuration.TotalDays}` days. Reason: ```{reason}```");
                await ctx.Guild.BanMemberAsync(user, 0, reason);
                var guild = db.Guilds.First(g => g.DiscordGuildId == ctx.Guild.Id);

                var bannedUser = db.Users.FirstOrDefault(u => u.UserId == user.Id);
                var formattedBanReason = Utilities.InfractionFormatHandler.ParseInfractionFormat("temporarily banned", banDuration.TotalDays + " days", user.Mention, reason, guild.InfractionFormat ?? defaultFormat);
                var infraction = CreateInfraction(formattedBanReason, ctx.User.Id, now);
                if (bannedUser is null)
                {
                    bannedUser = new DiscordUserInfo() { Infractions = new List<UserInfractionModel>() };
                    db.Users.Add(bannedUser);
                    bannedUser.Infractions.Add(infraction);
                }

                if (guild.GeneralLoggingChannel != default)
                {
                    embed.WithDescription(formattedBanReason);
                    embed.WithColor(DiscordColor.Green);
                    await ctx.Guild.GetChannel(guild.GeneralLoggingChannel.Value).SendMessageAsync(embed: embed);
                }
                SilkBot.Bot.Instance.Timer.Events.Add(new AppEvent
                {
                    EventType = InfractionType.TemporaryBan,
                    Expiration = DateTime.Now + banDuration,
                    Guild = ctx.Guild,
                    Id = user.Id,
                    Reason = reason
                });
            }
        }

        private async Task SendFailureMessage(CommandContext ctx, DiscordUser user, DiscordEmbedBuilder embed, BanFailureReason reason)
        {
            embed.WithDescription(reason.FailureReason.Replace("$user", user.Mention));
            embed.WithColor(DiscordColor.Red);
            await ctx.RespondAsync(embed: embed);
        }
        private UserInfractionModel CreateInfraction(string reason, ulong enforcerId, DateTime infractionTime)
        {
            return new UserInfractionModel
            {
                Reason = reason,
                Enforcer = enforcerId,
                InfractionTime = infractionTime,
                InfractionType = InfractionType.TemporaryBan,
            };
        }
        private DiscordEmbedBuilder GenerateBaseEmbed(DiscordUser user, string footerImageUrl, DateTime time)
        {
            return new DiscordEmbedBuilder()
                .WithAuthor(user.Username, null, user.AvatarUrl)
                .WithFooter($"Silk! | Enforcer: {user.Id}", footerImageUrl)
                .WithTimestamp(time);
        }
        private BanFailureReason CanBan(DiscordMember bot, DiscordMember caller, DiscordMember recipient)
        {

            if (!bot.Roles.Any(r => r.Permissions.HasFlag(Permissions.BanMembers)))
                return new BanFailureReason(BanFailureReason.INSUFFICIENT_BOT_PERMISSIONS);

            if (!caller.Roles.Any(r => r.Permissions.HasFlag(Permissions.BanMembers)))
                return new BanFailureReason(BanFailureReason.INSUFFICIENT_CALLER_PERMISSIONS);

            if (recipient.Hierarchy > bot.Hierarchy) return new BanFailureReason(BanFailureReason.UNSUITABLE_HIERARCHY_POSITION);

            if (recipient.Roles.Any(r => r.Permissions.HasPermission(Permissions.KickMembers)))
                return new BanFailureReason(BanFailureReason.MODERATOR_BAN_ATTEMPT);

            return null;
        }

        private TimeSpan GetTimeFromInput(string input) =>
            input.Contains('d') ? TimeSpan.FromDays(double.Parse(input[0..^1])) : throw new InvalidOperationException();


        public TempBanCommand() =>
            SilkBot.Bot.Instance.Timer.Dispatcher.UnBan += OnBanExpiration;


        private void OnBanExpiration(object sender, EventArgs e)
        {
            var actionObject = sender as AppEvent;
            actionObject.Guild.UnbanMemberAsync(actionObject.Id, "Temporary ban completed.");
        }
    }
}