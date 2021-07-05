#pragma warning disable CA1822 // Mark members as static

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Core.Utilities.HelpFormatter;
using Silk.Extensions;
using Silk.Extensions.DSharpPlus;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace Silk.Core.Commands.Miscellaneous
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
                .WithTitle($"Info for {role.Name} ({role.Id}):")
                .AddField("Color:", role.Color.ToString())
                .AddField("Created:", $"{Formatter.Timestamp(role.CreationTimestamp - DateTime.UtcNow, TimestampFormat.LongDateTime)} ({Formatter.Timestamp(role.CreationTimestamp - DateTime.UtcNow)})")
                .AddField("Hoisted:", role.IsHoisted.ToString())
                .AddField("Hierarchy:", GetHierarchy(ctx, role))
                .AddField("Bot role:", role.IsManaged.ToString())
                .AddField("Members:", memberString)
                .AddField("Mentionable:", role.IsMentionable.ToString())
                .AddField("Permissions:", role.Permissions.ToPermissionString())
                .WithColor(role.Color);
            await using var str = GetRoleColorStream();
            await ctx.RespondAsync(m => m.WithEmbed(embed).WithFile("roleColor.png", str));

            Stream GetRoleColorStream()
            {
                var str = new MemoryStream();
                using var bmp = new Bitmap(1280, 256);
                using var gfx = Graphics.FromImage(bmp);

                var r = role.Color;
                gfx.Clear(Color.FromArgb(r.R, r.G, r.B));

                bmp.Save(str, ImageFormat.Png);
                str.Position = 0;
                return str;
            }
        }
        
        [Command("info")]
        [Description("Get info about a member")]
        public async Task GetUserInfo(CommandContext ctx, DiscordMember member)
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithAuthor(member.ToDiscordName(), iconUrl: member.AvatarUrl)
                .WithDescription($"Information about {member.Mention}!")
                .WithColor(DiscordColor.Orange);

            var status = string.Empty;
            DiscordEmoji? emoji;

            try
            {
                emoji = GetPresenceEmoji(ctx.Client, member, out status);
            }
            catch
            {
                // If here, emoji wasn't able to be grabbed from Guild and threw an exception
                emoji = DiscordEmoji.FromName(ctx.Client, ":question:");
            }

            embed.AddField("Status:", $"{emoji}  {status}");
            embed.AddField("Name:", member.Username);
            embed.AddField("Creation Date:", $"{Formatter.Timestamp(member.CreationTimestamp - DateTime.UtcNow, TimestampFormat.LongDateTime)} ({Formatter.Timestamp(member.CreationTimestamp - DateTime.UtcNow)})");

            List<string> roleList = member.Roles
                .OrderByDescending(r => r.Position)
                .Select(role => role.Mention)
                .ToList();
            string roles = string.Join(' ', roleList);
            embed.AddField("Roles:", roles.Length < 1 ? "No roles." : roles);
            embed.AddField("Flags:", string.IsNullOrWhiteSpace(member.Flags.ToString()) ? "None" : member.Flags.ToString());
            embed.AddField("Bot:", member.IsBot.ToString());
            await ctx.RespondAsync(embed).ConfigureAwait(false);
        }

        private static string GetHierarchy(CommandContext ctx, DiscordRole role)
        {
            string roleString = string.Empty;
            var hasAboveRole = false;

            DiscordRole? rle;

            var roles = ctx.Guild.Roles.Values.OrderBy(r => r.Position).ToArray();

            if (roles.Any(r1 => r1.Position > role.Position))
            {
                hasAboveRole = true;
                rle = roles.First(r => r.Position > role.Position);
                roleString += $"{rle.Mention}\n";
            }

            roleString += $"{(hasAboveRole ? "⠀⠀↑" : "")}\n{role.Mention}\n";
            if (!roles.Any(r => r.Position < role.Position)) 
                return roleString;

            rle = roles.Last(r => r.Position < role.Position);
            roleString += $"⠀⠀↑\n{rle.Mention}";

            return roleString;
        }
        
        private static DiscordEmoji GetPresenceEmoji(DiscordClient client, DiscordUser member, out string status)
        {
            status = string.Empty;
            DiscordEmoji emoji = null!;
            switch (member.Presence?.Status)
            {
                case UserStatus.Online:
                    status = "Online";
                    return DiscordEmoji.TryFromGuildEmote(client, 743339430672203796, out emoji) ? emoji : DiscordEmoji.FromUnicode("🟢");
                case UserStatus.Idle:
                    status = "Away";
                    return DiscordEmoji.TryFromGuildEmote(client, 743339431720910889, out emoji) ? emoji : DiscordEmoji.FromUnicode("🟡");
                case UserStatus.DoNotDisturb:
                    status = "Do Not Disturb";
                    return DiscordEmoji.TryFromGuildEmote(client, 743339431632568450, out emoji) ? emoji : DiscordEmoji.FromUnicode("🔴");
                case UserStatus.Offline:
                    status = "Offline";
                    return DiscordEmoji.TryFromGuildEmote(client, 743339431905198100, out emoji) ? emoji : DiscordEmoji.FromUnicode("⚫");
                case UserStatus.Invisible:
                    break;
                case null:
                    break;
                default:
                    status = "Offline";
                    return DiscordEmoji.FromGuildEmote(client, 743339431905198100);
            }
            return emoji;
        }
    }
}