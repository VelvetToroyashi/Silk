using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SilkBot.Utilities;

namespace SilkBot.Commands.Miscellaneous
{
    [Category(Categories.Misc)]
    public class UserInfo : BaseCommandModule
    {
        [Command("info")]
        public async Task RoleInfo(CommandContext ctx, DiscordRole role)
        {
            var embed = new DiscordEmbedBuilder()
                .WithTitle($"Info for {role.Name} ( {role.Id} ) :")
                .AddField("Color:", role.Color.ToString())
                .AddField("Created:", role.CreationTimestamp.Date.ToShortDateString())
                .AddField("Hoisted?:", role.IsHoisted.ToString())
                .AddField("Bot role?:", role.IsManaged.ToString())
                .AddField("Permissions:", role.Permissions.ToString())
                .AddField("Mentionable?:", role.IsMentionable.ToString())
                .WithColor(role.Color)
                .WithThumbnail(ctx.Guild.IconUrl);
            await ctx.RespondAsync(embed: embed);
        }


        [Command("info")]
        public async Task GetUserInfo(CommandContext ctx, DiscordUser member)
        {
            var embed = new DiscordEmbedBuilder()
                .WithAuthor(member.Username, iconUrl: member.AvatarUrl)
                .WithDescription($"Information about {member.Mention}!")
                .WithColor(DiscordColor.Orange)
                .WithFooter("Silk!", ctx.Client.CurrentUser.AvatarUrl)
                .WithTimestamp(DateTime.Now);

            var status = "";
            DiscordEmoji emoji;

            try
            {
                switch (member.Presence?.Status)
                {
                    case UserStatus.Online:
                        status = "Online";
                        emoji = DiscordEmoji.FromGuildEmote(ctx.Client, 743339430672203796);
                        break;
                    case UserStatus.Idle:
                        status = "Idle";
                        emoji = DiscordEmoji.FromGuildEmote(ctx.Client, 743339431720910889);
                        break;
                    case UserStatus.DoNotDisturb:
                        status = "Do Not Disturb";
                        emoji = DiscordEmoji.FromGuildEmote(ctx.Client, 743339431632568450);
                        break;
                    case UserStatus.Offline:
                        status = "Offline";
                        emoji = DiscordEmoji.FromGuildEmote(ctx.Client, 743339431905198100);
                        break;
                    default:
                        status = "Offline";
                        emoji = DiscordEmoji.FromGuildEmote(ctx.Client, 743339431905198100);
                        break;
                }
            }
            catch (Exception ex)
            {
                // If here, emoji wasn't able to be grabbed from Guild and threw an exception
                emoji = DiscordEmoji.FromName(ctx.Client, ":question:");
            }

            embed.AddField("Status:", $"{emoji}  {status}");
            embed.AddField("Name:", member.Username);
            embed.AddField("Creation Date:", GetCreationTime(member.CreationTimestamp) + " ago");


            embed.AddField("Flags:", member.Flags.ToString());
            embed.AddField("Bot:", member.IsBot.ToString());
            await ctx.RespondAsync(embed: embed);
        }

        [Command("info")]
        public async Task GetUserInfo(CommandContext ctx, DiscordMember member)
        {
            var embed = new DiscordEmbedBuilder()
                .WithAuthor(member.DisplayName, iconUrl: member.AvatarUrl)
                .WithDescription($"Information about {member.Mention}!")
                .WithColor(DiscordColor.Orange)
                .WithFooter("Silk!", ctx.Client.CurrentUser.AvatarUrl)
                .WithTimestamp(DateTime.Now);

            var status = "";
            DiscordEmoji emoji = null;

            try
            {
                switch (member.Presence?.Status)
                {
                    case UserStatus.Online:
                        status = "Online";
                        emoji = DiscordEmoji.FromGuildEmote(ctx.Client, 743339430672203796);
                        break;
                    case UserStatus.Idle:
                        status = "Idle";
                        emoji = DiscordEmoji.FromGuildEmote(ctx.Client, 743339431720910889);
                        break;
                    case UserStatus.DoNotDisturb:
                        status = "Do Not Disturb";
                        emoji = DiscordEmoji.FromGuildEmote(ctx.Client, 743339431632568450);
                        break;
                    case UserStatus.Offline:
                        status = "Offline";
                        emoji = DiscordEmoji.FromGuildEmote(ctx.Client, 743339431905198100);
                        break;
                    default:
                        status = "Offline";
                        emoji = DiscordEmoji.FromGuildEmote(ctx.Client, 743339431905198100);
                        break;
                }
            }
            catch (Exception ex)
            {
                // If here, emoji wasn't able to be grabbed from Guild and threw an exception
                emoji = DiscordEmoji.FromName(ctx.Client, ":question:");
            }

            embed.AddField("Status:", $"{emoji}  {status}");
            embed.AddField("Name:", member.Username);
            embed.AddField("Creation Date:", GetCreationTime(member.CreationTimestamp) + " ago");

            var roleList = member.Roles
                .OrderByDescending(r => r.Position)
                .Select(role => role.Mention)
                .ToList();
            var roles = string.Join(' ', roleList);
            embed.AddField("Roles:", roles.Count() < 1 ? "No roles." : roles);
            embed.AddField("Flags:", member.Flags.ToString());
            embed.AddField("Bot:", member.IsBot.ToString());
            await ctx.RespondAsync(embed: embed);
        }

        private string GetCreationTime(DateTimeOffset offset)
        {
            var creationTime = DateTime.Now.Subtract(offset.DateTime);
            var sb = new StringBuilder();
            if (creationTime.Days > 365)
            {
                var years = creationTime.Days / 365;
                sb.Append($"{years} {(years > 1 ? "years" : "year")}, ");
                creationTime = creationTime.Subtract(TimeSpan.FromDays(years * 365));
            }
            if (creationTime.Days > 30)
            {
                var months = creationTime.Days / 30;
                sb.Append($"{months} {(months > 1 ? "months" : "month")}, ");
                creationTime = creationTime.Subtract(TimeSpan.FromDays(months * 30));
            }
            sb.Append($"{creationTime.Days} {(creationTime.Days > 1 ? "days" : "day")}");

            return sb.ToString();

        }
    }
}
