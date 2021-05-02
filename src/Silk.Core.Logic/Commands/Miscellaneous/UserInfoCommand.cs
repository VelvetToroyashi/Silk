#pragma warning disable CA1822 // Mark members as static

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Core.Discord.Utilities.HelpFormatter;
using Silk.Extensions;

namespace Silk.Core.Logic.Commands.Miscellaneous
{
    [Category(Categories.Misc)]
    public class UserInfo : BaseCommandModule
    {
        [Command("roleinfo")]
        [Aliases("role_info", "role-info")]
        [Description("Get info about a role")]
        public async Task RoleInfo(CommandContext ctx, DiscordRole role)
        {
            IEnumerable<DiscordMember> members = ctx.Guild.Members.Values.Where(m => m.Roles.Contains(role));
            string memberString = members.Count() is 0 && role != ctx.Guild.EveryoneRole ?
                "This role isn't assigned to anyone!" :
                members.Take(members.Count() > 5 ? 5 :
                        members.Count())
                    .Select(m => m.Mention)
                    .Join(", ") + $"{(role == ctx.Guild.EveryoneRole ? "Everyone has the @everyone role!" : members.Count() > 5 ? $" (plus ...{members.Count() - 5} others)" : null)}";

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithTitle($"Info for {role.Name} ( {role.Id} ):")
                .AddField("Color:", role.Color.ToString())
                .AddField("Created:", role.CreationTimestamp.Date.ToShortDateString())
                .AddField("Hoisted:", role.IsHoisted.ToString())
                .AddField("Hierarchy:", GetHierarchy(ctx, role))
                .AddField("Bot role:", role.IsManaged.ToString())
                .AddField("Members:", memberString)
                .AddField("Mentionable:", role.IsMentionable.ToString())
                .AddField("Permissions:", role.Permissions.ToPermissionString())
                .WithColor(role.Color)
                .WithThumbnail(ctx.Guild.IconUrl);

            await ctx.RespondAsync(embed);
        }

        [Command("info")]
        [Description("Get info about a member")]
        public async Task GetUserInfo(CommandContext ctx, DiscordUser member)
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithAuthor(member.Username, iconUrl: member.AvatarUrl)
                .WithDescription($"Information about {member.Mention}!")
                .WithColor(DiscordColor.Orange);

            var status = string.Empty;
            DiscordEmoji? emoji = null;

            try
            {
                emoji = GetPresenceEmoji(ctx.Client, member, out status);
            }
            catch (Exception)
            {
                // If here, emoji wasn't able to be grabbed from Guild and threw an exception
                emoji = DiscordEmoji.FromName(ctx.Client, ":question:");
            }

            embed.AddField("Status:", $"{emoji}  {status}");
            embed.AddField("Name:", member.Username);
            embed.AddField("Creation Date:", GetCreationTime(member.CreationTimestamp) + " ago");


            embed.AddField("Flags:", member.Flags.ToString() == "" ? "None" : member.Flags.ToString());
            embed.AddField("Bot:", member.IsBot.ToString());
            await ctx.RespondAsync(embed);
        }

        [Command("info")]
        [Description("Get info about a member")]
        public async Task GetUserInfo(CommandContext ctx, DiscordMember member)
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithAuthor(member.DisplayName, iconUrl: member.AvatarUrl)
                .WithDescription($"Information about {member.Mention}!")
                .WithColor(DiscordColor.Orange);

            var status = string.Empty;
            DiscordEmoji? emoji = null;

            try
            {
                emoji = GetPresenceEmoji(ctx.Client, member, out status);
            }
            catch (Exception)
            {
                // If here, emoji wasn't able to be grabbed from Guild and threw an exception
                emoji = DiscordEmoji.FromName(ctx.Client, ":question:");
            }

            embed.AddField("Status:", $"{emoji}  {status}");
            embed.AddField("Name:", member.Username);
            embed.AddField("Creation Date:", GetCreationTime(member.CreationTimestamp) + " ago");

            List<string> roleList = member.Roles
                .OrderByDescending(r => r.Position)
                .Select(role => role.Mention)
                .ToList();
            string roles = string.Join(' ', roleList);
            embed.AddField("Roles:", roles.Length < 1 ? "No roles." : roles);
            embed.AddField("Flags:", member.Flags.ToString());
            embed.AddField("Bot:", member.IsBot.ToString());
            await ctx.RespondAsync(embed).ConfigureAwait(false);
        }

        private static string GetHierarchy(CommandContext ctx, DiscordRole role)
        {
            string roleString = string.Empty;
            bool hasAboveRole = false;

            DiscordRole? rle = null;

            IOrderedEnumerable<DiscordRole> roles = ctx.Guild.Roles.Values.OrderBy(r => r.Position);

            if (roles.Any(r => r.Position > role.Position))
            {
                hasAboveRole = true;
                rle = roles.First(r => r.Position > role.Position);
                roleString += $"{rle.Mention}\n";
            }

            roleString += $"{(hasAboveRole ? "⠀⠀↑" : "")}\n{role.Mention}\n";
            if (roles.Any(r => r.Position < role.Position))
            {
                rle = roles.Last(r => r.Position < role.Position);
                roleString += $"⠀⠀↑\n{rle.Mention}";
            }

            return roleString;
        }

        private static string GetCreationTime(DateTimeOffset offset)
        {
            TimeSpan creationTime = DateTime.Now.Subtract(offset.DateTime);
            var sb = new StringBuilder();
            if (creationTime.Days > 365)
            {
                int years = creationTime.Days / 365;
                sb.Append($"{years} {(years > 1 ? "years" : "year")}, ");
                creationTime = creationTime.Subtract(TimeSpan.FromDays(years * 365));
            }

            if (creationTime.Days > 30)
            {
                int months = creationTime.Days / 30;
                sb.Append($"{months} {(months > 1 ? "months" : "month")}, ");
                creationTime = creationTime.Subtract(TimeSpan.FromDays(months * 30));
            }

            sb.Append($"{creationTime.Days} {(creationTime.Days > 1 ? "days" : "day")}");

            return sb.ToString();
        }

        private static DiscordEmoji GetPresenceEmoji(DiscordClient client, DiscordUser member, out string status)
        {
            status = string.Empty;
            switch (member.Presence?.Status)
            {
                case UserStatus.Online:
                    status = "Online";
                    return DiscordEmoji.FromGuildEmote(client, 743339430672203796);
                case UserStatus.Idle:
                    status = "Away";
                    return DiscordEmoji.FromGuildEmote(client, 743339431720910889);
                case UserStatus.DoNotDisturb:
                    status = "Do Not Disturb";
                    return DiscordEmoji.FromGuildEmote(client, 743339431632568450);
                case UserStatus.Offline:
                    status = "Offline";
                    return DiscordEmoji.FromGuildEmote(client, 743339431905198100);
                default:
                    status = "Offline";
                    return DiscordEmoji.FromGuildEmote(client, 743339431905198100);
            }
        }
    }
}