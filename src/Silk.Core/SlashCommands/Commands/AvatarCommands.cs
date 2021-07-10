using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Silk.Extensions.DSharpPlus;

namespace Silk.Core.SlashCommands.Commands
{
	public enum AvatarOption
	{
		[ChoiceName("guild-specific")]
		Guild,
		[ChoiceName("user-specific")]
		User
	}
	
	public class AvatarCommands : SlashCommandModule
	{
		[SlashCommand("avatar", "View someone's avatar!")]
		public async Task Avatar(
			InteractionContext ctx,
			[Option("user", "Who's avatar do you want to see")] DiscordUser? user = null,
			[Option("type", "What avatar should I pull?")] AvatarOption avatarOption = AvatarOption.User,
			[Option("visibility", "Do you want others to see this command?")] 
			bool asEphemeral = true)
		{
			await ctx.CreateThinkingResponseAsync(!asEphemeral);
			user ??= ctx.Member ?? ctx.User;

			switch (avatarOption)
			{
				case AvatarOption.Guild when ctx.Interaction.GuildId is null:
					await ctx.EditResponseAsync(new() {Content = "I can't get someone's guild-avatar from DMs, silly!"});
					return;
				case AvatarOption.Guild when ctx.Guild is null:
					await ctx.EditResponseAsync(new DiscordWebhookBuilder()
						.WithContent("Sorry, but I need to be auth'd with the bot scope to retrieve guild-specific avatars!")
						.AddComponents(new DiscordLinkButtonComponent($"https://discord.com/oauth2/authorize?client_id={ctx.Client.CurrentApplication.Id}&permissions=502656214&scope=bot%20applications.commands", "Invite with bot scope")));
					return;
				case AvatarOption.Guild when ctx.Guild.Members.ContainsKey(user.Id):
				{
					if (ctx.Guild.Members[user.Id].GuildAvatarHash != user.AvatarHash)
						await SendGuildAvatar();
					else await SendNoAvatarMessage();
					break;
				}
				case AvatarOption.Guild:
					await SendNonMemberMessage();
					break;
				default:
					await SendUserAvatar();
					break;
			}

			Task SendNoAvatarMessage() => ctx.EditResponseAsync(new() {Content = "Sorry, but that user doesn't exist on the server!"});
			Task SendNonMemberMessage() => ctx.EditResponseAsync(new() {Content = "Sorry, but that user doesn't exist on the server!"});

			Task SendUserAvatar() => ctx.EditResponseAsync(new
					DiscordWebhookBuilder()
				.AddEmbed(new DiscordEmbedBuilder()
					.WithColor(DiscordColor.CornflowerBlue)
					.WithTitle($"{user!.Username}'s Avatar:")
					.WithImageUrl(user.AvatarUrl)));

			Task SendGuildAvatar() => ctx.EditResponseAsync(new
					DiscordWebhookBuilder()
				.AddEmbed(new DiscordEmbedBuilder()
					.WithColor(DiscordColor.CornflowerBlue)
					.WithTitle($"{user.Username}'s Guild-Specific Avatar:")
					.WithAuthor(((DiscordMember) user).DisplayName, iconUrl: user.AvatarUrl)
					.WithImageUrl(((DiscordMember) user).GuildAvatarUrl)));
		}
	}
}