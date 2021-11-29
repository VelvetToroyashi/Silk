#pragma warning disable CA1822 // Mark members as static

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Humanizer;
using Silk.Core.Utilities.HelpFormatter;
using Silk.Extensions;
using Silk.Extensions.DSharpPlus;
using Silk.Shared.Constants;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Silk.Core.Commands.Miscellaneous
{
	[HelpCategory(Categories.Misc)]
	public class UserInfo : BaseCommandModule
	{
		[Command("roleinfo")]
		[Aliases("role_info", "role-info")]
		[Description("Get info about a role")]
		public async Task RoleInfo(CommandContext ctx, DiscordRole role)
		{
			var members = ctx.Guild.Members.Values.Where(m => m.Roles.Contains(role));
			var memberString = GetRoleMemberCount(ctx, role, members);

			var ms = await GenerateColoredImageAsync(role.Color);

			DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
				.WithTitle($"Info for {role.Name} ({role.Id}):")
				.WithImageUrl("attachment://color.png")
				.AddField("Color:", role.Color.ToString())
				.AddField("Created:", $"{Formatter.Timestamp(role.CreationTimestamp - DateTime.UtcNow, TimestampFormat.LongDateTime)} ({Formatter.Timestamp(role.CreationTimestamp - DateTime.UtcNow)})")
				.AddField("Hoisted:", role.IsHoisted.ToString())
				.AddField("Hierarchy:", GetHierarchy(ctx, role))
				.AddField("Bot role:", role.IsManaged.ToString())
				.AddField("Members:", memberString)
				.AddField("Mentionable:", role.IsMentionable.ToString())
				.AddField("Permissions:", role.Permissions.ToPermissionString())
				.WithColor(role.Color);

			await ctx.RespondAsync(m => m.WithEmbed(embed).WithFile("color.png", ms));
		}

		[Aliases("userinfo")]
		[Command("info")]
		public async Task GetUserInfo(CommandContext ctx, DiscordUser user)
		{
			var embed = new DiscordEmbedBuilder()
				.WithColor(DiscordColor.Orange)
				.WithAuthor(user.ToDiscordName(), iconUrl: user.AvatarUrl)
				.WithDescription($"Information about {user.Mention}!")
				.WithFooter("⚠This member is not a part of this server, thus API information is limited.⚠");

			embed.AddField("Joined Discord:", $"{Formatter.Timestamp(user.CreationTimestamp, TimestampFormat.LongDateTime)} ({Formatter.Timestamp(user.CreationTimestamp)})");
			embed.AddField("Flags:", user.Flags?.Humanize(LetterCasing.Title) ?? "None");
			embed.AddField("Bot:", user.IsBot ? "Yes" : "No");

			embed.WithThumbnail(user.AvatarUrl ?? user.DefaultAvatarUrl);

			if (!string.IsNullOrEmpty(user.BannerHash))
			{
				embed.WithImageUrl(user.BannerUrl);
				await ctx.RespondAsync(embed);
			}
			else if (user.BannerColor.HasValue)
			{
				await using var banner = await GenerateColoredImageAsync(user.BannerColor.Value);

				embed.WithImageUrl("attachment://banner.png");

				var builder = new DiscordMessageBuilder().WithEmbed(embed).WithFile("banner.png", banner);
				await ctx.RespondAsync(builder);
			}
			else
			{
				await ctx.RespondAsync(embed);
			}
		}

		[Priority(2)]
		[Command("info")]
		[Description("Get info about someone")]
		public async Task GetMemberInfo(CommandContext ctx, DiscordMember member)
		{
			(typeof(BaseDiscordClient)
				.GetProperty("UserCache", BindingFlags.NonPublic | BindingFlags.Instance)!
				.GetValue(ctx.Client) as ConcurrentDictionary<ulong, DiscordUser>)!.TryRemove(member.Id, out _);

			var user = await ctx.Client.GetUserAsync(member.Id);

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
			embed.AddField("Flags:", member.Flags?.Humanize(LetterCasing.Title) ?? "None");
			embed.AddField("Bot:", member.IsBot ? "Yes" : "No");


			embed.WithThumbnail(member.AvatarUrl ?? member.DefaultAvatarUrl);

			if (!string.IsNullOrEmpty(user.BannerHash))
			{
				embed.WithImageUrl(member.BannerUrl);
				await ctx.RespondAsync(embed);
			}
			else if (user.BannerColor.HasValue)
			{
				await using var banner = await GenerateColoredImageAsync(user.BannerColor.Value);

				embed.WithImageUrl("attachment://banner.png");

				var builder = new DiscordMessageBuilder().WithEmbed(embed).WithFile("banner.png", banner);
				await ctx.RespondAsync(builder);
			}
			else
			{
				await ctx.RespondAsync(embed);
			}
		}

		private static async Task<MemoryStream> GenerateColoredImageAsync(DiscordColor color)
		{
			using var colorImage = new Image<Rgba32>(600, 200, Rgba32.ParseHex(color.ToString()));
			var ms = new MemoryStream();

			await colorImage.SaveAsPngAsync(ms);
			ms.Position = 0;
			return ms;
		}

		private static string GetRoleMemberCount(CommandContext ctx, DiscordRole role, IEnumerable<DiscordMember> members)
		{
			if (role == ctx.Guild.EveryoneRole)
				return "Everyone has the @everyone role!";

			var memberCount = members.Count();

			if (memberCount is 0)
				return "This role isn't assigned to anyone!";

			members = members.Take(5);

			return $"{members.Select(m => m.Mention).Join(", ")} {(memberCount > 5 ? $"(...Plus {memberCount - 5} others.)" : null)}";
		}


		private static string GetHierarchy(CommandContext ctx, DiscordRole role)
		{
			var roleStringBuilder = new StringBuilder();

			foreach (var r in ctx.Guild.Roles.Values.OrderByDescending(r => r.Position))
			{
				if (r.Position + 1 == role.Position)
				{
					roleStringBuilder
						.AppendLine("\t↑")
						.AppendLine(r.Mention);
				}
				else if (r.Position == role.Position)
				{
					roleStringBuilder.AppendLine(role.Mention);
				}
				else if (r.Position - 1 == role.Position)
				{
					roleStringBuilder
						.AppendLine(r.Mention)
						.AppendLine("\t↑");
				}
			}
			return roleStringBuilder.ToString();
		}

		private static DiscordEmoji GetPresenceEmoji(DiscordClient client, DiscordUser member, out string status)
		{
			status = string.Empty;
			DiscordEmoji emoji;
			switch (member.Presence?.Status)
			{
				case UserStatus.Online:
					status = "Online";
					return DiscordEmoji.TryFromGuildEmote(client, Emojis.OnlineId, out emoji) ? emoji : DiscordEmoji.FromUnicode("🟢");
				case UserStatus.Idle:
					status = "Away";
					return DiscordEmoji.TryFromGuildEmote(client, Emojis.AwayId, out emoji) ? emoji : DiscordEmoji.FromUnicode("🟡");
				case UserStatus.DoNotDisturb:
					status = "Do Not Disturb";
					return DiscordEmoji.TryFromGuildEmote(client, Emojis.DoNotDisturbId, out emoji) ? emoji : DiscordEmoji.FromUnicode("🔴");
				default:
					status = "Offline";
					return DiscordEmoji.TryFromGuildEmote(client, Emojis.OfflineId, out emoji) ? emoji : DiscordEmoji.FromUnicode("⚫");
			}
		}
	}
}