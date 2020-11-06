using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using SilkBot.Extensions;
using SilkBot.Models;
using SilkBot.Tools;
using SilkBot.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SilkBot.Commands.Moderation.Ban
{
    public class TempBanCommand : BaseCommandModule
    {
        public IDbContextFactory<SilkDbContext> DbFactory   { private get; set; }
        public TimedEventService EventService               { private get; set; }
        public DiscordClient Client                         { private get; set; }

        private const string defaultFormat = "$mention was $action from the guild for $duration for $reason";
        [Command("tempban"), RequireGuild()]
        public async Task TempBan(CommandContext ctx, DiscordMember user, string duration, [RemainingText] string reason = "Not provided.")
        {
            using var db = DbFactory.CreateDbContext();
            var bot = ctx.Guild.CurrentMember;
            var now = DateTime.Now;
            var banDuration = GetTimeFromInput(duration);
            var banFailed = CanBan(bot, ctx.Member, user); 
            var embed = new DiscordEmbedBuilder().WithAuthor(bot.Username, ctx.GetBotUrl(), ctx.Client.CurrentUser.AvatarUrl);

            if (!(banFailed is null))
            {
                await SendFailureMessage(ctx, user, embed, banFailed);
                return;
            }
            else
            {
                var banEmbed = new DiscordEmbedBuilder()
                    .WithAuthor(ctx.User.Username, ctx.User.GetUrl(), ctx.User.AvatarUrl)
                    .WithDescription($"You've been temporarily banned from {ctx.Guild.Name} for {duration} days.")
                    .AddField("Reason:", reason);

                await user.SendMessageAsync(embed: banEmbed);
                await ctx.Guild.BanMemberAsync(user, 0, reason);
                var guild = db.Guilds.First(g => g.DiscordGuildId == ctx.Guild.Id);
                
                UserInfoModel bannedUser = db.Users.FirstOrDefault(u => u.UserId == user.Id);
                string? formattedBanReason = Utilities.InfractionFormatHandler.ParseInfractionFormat("temporarily banned", banDuration.TotalDays + " days", user.Mention, reason, guild.InfractionFormat ?? defaultFormat);
                UserInfractionModel? infraction = CreateInfraction(formattedBanReason, ctx.User.Id, now);
                if (bannedUser is null)
                {
                    bannedUser = new UserInfoModel() { Infractions = new List<UserInfractionModel>() };
                    db.Users.Add(bannedUser);
                    bannedUser.Infractions.Add(infraction);
                }

                if (guild.GeneralLoggingChannel != default)
                {
                    embed.WithDescription(formattedBanReason);
                    embed.WithColor(DiscordColor.Green);
                    await ctx.Guild.GetChannel(guild.GeneralLoggingChannel).SendMessageAsync(embed: embed);
                }
                EventService.Events.Add(new TimedInfraction(user.Id, ctx.Guild.Id, DateTime.Now.Add(banDuration), reason, (e) => OnBanExpiration((TimedInfraction)e)));
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

        /// <summary>
        /// Check if the bot can actually ban the specified member.
        /// </summary>
        /// <param name="bot">The bot, which actually bans the member.</param>
        /// <param name="caller">The member that's executing the command</param>
        /// <param name="recipient">The member to be banned.</param>
        /// <returns>A <see cref="BanFailureReason"></see> if some check fails, else null.</returns>
        private BanFailureReason CanBan(DiscordMember bot, DiscordMember caller, DiscordMember recipient)
        {


            if (!bot.Roles.Any(r => r.Permissions.HasFlag(Permissions.BanMembers)))
                return new BanFailureReason(BanFailureReason.INSUFFICIENT_BOT_PERMISSIONS);

            else if (!caller.Roles.Any(r => r.Permissions.HasFlag(Permissions.BanMembers)))
                return new BanFailureReason(BanFailureReason.INSUFFICIENT_CALLER_PERMISSIONS);

            else if (recipient.Hierarchy > bot.Hierarchy) return new BanFailureReason(BanFailureReason.UNSUITABLE_HIERARCHY_POSITION);

            else if (recipient.Roles.Any(r => r.Permissions.HasPermission(Permissions.KickMembers)))
                return new BanFailureReason(BanFailureReason.MODERATOR_BAN_ATTEMPT);

            else return null;
        }

        private TimeSpan GetTimeFromInput(string input) =>
            input.Contains('d') ? 
                double.TryParse(input[0..^1], out var dur) ? 
            TimeSpan.FromDays(dur) :
                throw new InvalidOperationException("Couldn't determine duration from message!") :
                throw new InvalidOperationException("Couldn't determine duration from message!");


        private async Task OnBanExpiration(TimedInfraction eventObject)
        {
            var db = DbFactory.CreateDbContext();
            GuildModel guild = db.Guilds.First(g => g.DiscordGuildId == eventObject.Guild);
            if(guild.GeneralLoggingChannel != default)
            {
                DiscordChannel c = (await Client.GetGuildAsync(eventObject.Guild)).GetChannel(guild.GeneralLoggingChannel);
                DiscordUser u = await Client.GetUserAsync(eventObject.Id);
                var embed = new DiscordEmbedBuilder().WithDescription($"{u.Mention}'s ban has expired.").WithColor(DiscordColor.PhthaloGreen).WithThumbnail(u.AvatarUrl);
            }
            await (await Client.GetGuildAsync(eventObject.Guild)).UnbanMemberAsync(eventObject.Id, "Temporary ban completed.");
        }
    }
}