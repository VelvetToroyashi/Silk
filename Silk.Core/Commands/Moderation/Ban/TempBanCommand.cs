using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Commands.Moderation.Utilities;
using Silk.Core.Database;
using Silk.Core.Database.Models;
using Silk.Core.Tools;
using Silk.Core.Utilities;
using Silk.Extensions;
using Silk.Extensions.DSharpPlus;

namespace Silk.Core.Commands.Moderation.Ban
{
    [Category(Categories.Mod)]
    public class TempBanCommand : BaseCommandModule
    {
        //TODO: Clean this up
        public IDbContextFactory<SilkDbContext> DbFactory { private get; set; }
        public TimedEventService EventService { private get; set; }
        public DiscordClient Client { private get; set; }

        private const string defaultFormat = "$mention was $action from the guild for $duration for $reason";

        [Command("tempban")]
        [RequireGuild]
        public async Task TempBan(CommandContext ctx, DiscordMember user, string duration, [RemainingText] string reason = "Not provided.")
        {
            SilkDbContext db = DbFactory.CreateDbContext();
            DiscordMember bot = ctx.Guild.CurrentMember;
            DateTime now = DateTime.Now;
            TimeSpan banDuration = GetTimeFromInput(duration);
            BanFailureReason? banFailed = CanBan(bot, ctx.Member, user);
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder().WithAuthor(bot.Username, ctx.GetBotUrl(), ctx.Client.CurrentUser.AvatarUrl);

            if (banFailed is not null)
            {
                await SendFailureMessage(ctx, user, embed, banFailed);
            }
            else
            {
                DiscordEmbedBuilder banEmbed = new DiscordEmbedBuilder()
                    .WithAuthor(ctx.User.Username, ctx.User.GetUrl(), ctx.User.AvatarUrl)
                    .WithDescription($"You've been temporarily banned from {ctx.Guild.Name} for {duration} days.")
                    .AddField("Reason:", reason);

                await user.SendMessageAsync(embed: banEmbed);
                await ctx.Guild.BanMemberAsync(user, 0, reason);
                
                GuildModel guild = db.Guilds.First(g => g.Id == ctx.Guild.Id);

                UserModel? bannedUser = db.Users.FirstOrDefault(u => u.Id == user.Id);
                string formattedBanReason = InfractionFormatHandler.ParseInfractionFormat("temporarily banned",
                    banDuration.TotalDays + " days", user.Mention, reason,
                    guild.Configuration.InfractionFormat ?? defaultFormat);
                
                UserInfractionModel infraction = CreateInfraction(formattedBanReason, ctx.User.Id, now);
                if (bannedUser is null)
                {
                    bannedUser = new UserModel
                    {
                        Infractions = new List<UserInfractionModel>()
                    };
                    
                    db.Users.Add(bannedUser);
                    bannedUser.Infractions.Add(infraction);
                }

                if (guild.Configuration.GeneralLoggingChannel != default)
                {
                    embed.WithDescription(formattedBanReason);
                    embed.WithColor(DiscordColor.Green);
                    await ctx.Guild.GetChannel(guild.Configuration.GeneralLoggingChannel).SendMessageAsync(embed: embed);
                }

                EventService.Events.Add(new TimedInfraction(user.Id, ctx.Guild.Id, DateTime.Now.Add(banDuration),
                    reason, e => _ = OnBanExpiration((TimedInfraction) e)));
            }
        }

        private static async Task SendFailureMessage(CommandContext ctx, DiscordUser user, DiscordEmbedBuilder embed, BanFailureReason reason)
        {
            embed.WithDescription(reason.FailureReason.Replace("$user", user.Mention));
            embed.WithColor(DiscordColor.Red);
            await ctx.RespondAsync(embed: embed);
        }

        private static UserInfractionModel CreateInfraction(string reason, ulong enforcerId, DateTime infractionTime)
        {
            return new()
            {
                Reason = reason,
                Enforcer = enforcerId,
                InfractionTime = infractionTime,
                InfractionType = Database.Models.InfractionType.SoftBan
            };
        }

        /// <summary>
        /// Check if the bot can actually ban the specified member.
        /// </summary>
        /// <param name="bot">The bot, which actually bans the member.</param>
        /// <param name="caller">The member that's executing the command</param>
        /// <param name="recipient">The member to be banned.</param>
        /// <returns>A <see cref="BanFailureReason"></see> if some check fails, else null.</returns>
        private static BanFailureReason CanBan(DiscordMember bot, DiscordMember caller, DiscordMember recipient)
        {
            if (!bot.Roles.Any(r => r.Permissions.HasFlag(Permissions.BanMembers)))
                return new BanFailureReason(BanFailureReason.INSUFFICIENT_BOT_PERMISSIONS);

            if (!caller.Roles.Any(r => r.Permissions.HasFlag(Permissions.BanMembers)))
                return new BanFailureReason(BanFailureReason.INSUFFICIENT_CALLER_PERMISSIONS);

            if (recipient.Hierarchy > bot.Hierarchy)
                return new BanFailureReason(BanFailureReason.UNSUITABLE_HIERARCHY_POSITION);

            if (recipient.Roles.Any(r => r.Permissions.HasPermission(Permissions.KickMembers)))
                return new BanFailureReason(BanFailureReason.MODERATOR_BAN_ATTEMPT);

            return null;
        }

        private static TimeSpan GetTimeFromInput(string input)
        {
            return input.Contains('d')
                ? double.TryParse(input[..^1], out double dur) ? TimeSpan.FromDays(dur) :
                throw new InvalidOperationException("Couldn't determine duration from message!")
                : throw new InvalidOperationException("Couldn't determine duration from message!");
        }


        private async Task OnBanExpiration(TimedInfraction eventObject)
        {
            SilkDbContext db = DbFactory.CreateDbContext();
            GuildModel guild = db.Guilds.First(g => g.Id == eventObject.Guild);
            if (guild.Configuration.GeneralLoggingChannel != default)
            {
                DiscordChannel c = (await Client.GetGuildAsync(eventObject.Guild))
                    .GetChannel(guild.Configuration.GeneralLoggingChannel);
                
                DiscordUser u = await Client.GetUserAsync(eventObject.Id);
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder().WithDescription($"{u.Mention}'s ban has expired.")
                    .WithColor(DiscordColor.PhthaloGreen)
                    .WithThumbnail(u.AvatarUrl);
            }

            await (await Client.GetGuildAsync(eventObject.Guild)).UnbanMemberAsync(eventObject.Id,
                "Temporary ban completed.");
        }
    }
}