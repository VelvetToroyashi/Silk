using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

namespace RoleMenuPlugin
{
	/// <summary>
	/// The command module responsible for creating, modifying, and deleting role menus.
	/// </summary>
	public sealed class RoleMenuCommand : BaseCommandModule
	{
		[Command]
		public async Task Create(CommandContext ctx, DiscordChannel channel)
		{
			if (!channel.PermissionsFor(ctx.Guild.CurrentMember).HasPermission(Permissions.SendMessages))
			{
				await ctx.RespondAsync("Sorry, but I can't send messages to that channel! Try a different one!");
				return;
			}
			
			var interactivity = ctx.Client.GetInteractivity();

			var addRoleButton = new DiscordButtonComponent(ButtonStyle.Primary, "add role", "Add a role");
			var addEmojiButton = new DiscordButtonComponent(ButtonStyle.Primary, "add emoji", "Add an emoji");
			var completeButton = new DiscordButtonComponent(ButtonStyle.Success, "complete", "Finish", false, new("✔"));
			var exitButton = new DiscordButtonComponent(ButtonStyle.Danger, "quit", "Quit", false, new("❌"));
			
			var message = await ctx.Channel
				.SendMessageAsync(m => m.WithContent("Role menu setup: What would you like to do?")
					.AddComponents(addEmojiButton, addRoleButton)
					.AddComponents(completeButton, exitButton));

			
			while (false)
			{
				var res = await interactivity.WaitForButtonAsync(message, ctx.User, CancellationToken.None);
				if (res.Result.Id == exitButton.CustomId)
				{
					await res.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
					await res.Result.Interaction.DeleteOriginalResponseAsync();
					return;
				}
				
			}
		}
	}
}