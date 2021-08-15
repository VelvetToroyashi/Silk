using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using MediatR;
using RoleMenuPlugin.Database;

namespace RoleMenuPlugin
{
	/// <summary>
	/// The command module responsible for creating, modifying, and deleting role menus.
	/// </summary>
	public sealed class RoleMenuCommand : BaseCommandModule
	{
		private readonly IMediator _mediator;
		public RoleMenuCommand(IMediator mediator) => _mediator = mediator;

		[Command]
		public async Task Create(CommandContext ctx, DiscordChannel channel)
		{
			if (!channel.PermissionsFor(ctx.Guild.CurrentMember).HasPermission(Permissions.SendMessages))
			{
				await ctx.RespondAsync("Sorry, but I can't send messages to that channel! Try a different one!");
				return;
			}
			
			var interactivity = ctx.Client.GetInteractivity();

			var roles = new List<DiscordRole>();
			var emoji = new List<DiscordComponentEmoji>();

			var addRoleButton = new DiscordButtonComponent(ButtonStyle.Primary, "add role", "Add a role");
			var addEmojiButton = new DiscordButtonComponent(ButtonStyle.Primary, "add emoji", "Add an emoji");
			var completeButton = new DiscordButtonComponent(ButtonStyle.Success, "complete", "Finish", true, new("✔"));
			var exitButton = new DiscordButtonComponent(ButtonStyle.Danger, "quit", "Quit", false, new("❌"));
			
			var message = await ctx.Channel
				.SendMessageAsync(m => m.WithContent("Role menu setup: What would you like to do?")
					.AddComponents(addEmojiButton, addRoleButton)
					.AddComponents(completeButton, exitButton));
			
			while (true)
			{
				if (roles.Count > 1)
					completeButton.Enable();
				
				await message.ModifyAsync(m => m.WithContent("Role menu setup: What would you like to do?")
					.AddComponents(addEmojiButton, addRoleButton)
					.AddComponents(completeButton, exitButton));
				
				var res = await interactivity.WaitForButtonAsync(message, ctx.User, CancellationToken.None);
				if (res.Result.Id == exitButton.CustomId)
				{
					await res.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
					await res.Result.Interaction.DeleteOriginalResponseAsync();
					return;
				}

				if (res.Result.Id == completeButton.CustomId)
				{
					await res.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
					break;
				}

				if (res.Result.Id == addEmojiButton.CustomId)
				{
					addRoleButton.Disable();
					addEmojiButton.Disable();
					
					await res.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder()
						.WithContent(res.Result.Message.Content)
						.AddComponents(addEmojiButton, addRoleButton)
						.AddComponents(completeButton, exitButton));

					var emojiInput = await GetEmojiInputAsync(ctx.Client, message, ctx.User, interactivity);

					if (emojiInput is null)
						continue;
					
					emoji.Add(emojiInput);
					addRoleButton.Enable();
				}
				else
				{
					addRoleButton.Disable();
					addEmojiButton.Disable();
					
					await res.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder()
						.WithContent(res.Result.Message.Content)
						.AddComponents(addEmojiButton, addRoleButton)
						.AddComponents(completeButton, exitButton));

					var roleInput = await GetRoleInputAsync(ctx, message, ctx.User, interactivity);
					
					roles.Add(roleInput);
					
					if (emoji.Count < roles.Count)
						emoji.Add(null);
					
					addRoleButton.Enable();
					addEmojiButton.Enable();
				}
			}

			var initiateButton = new DiscordButtonComponent(ButtonStyle.Primary, RoleMenuRoleService.RoleMenuPrefix, "Get roles!");
			
			message = await channel.SendMessageAsync(m => m
				.WithContent($"**Role Menu**\nAvailble roles: \n{string.Join('\n', roles.Select(r => r.Mention))}")
				.WithAllowedMentions(Mentions.None)
				.AddComponents(initiateButton));

			await FlushAndSaveChangesAsync(message, roles, emoji);
		}
		
		private async Task FlushAndSaveChangesAsync(DiscordMessage message, List<DiscordRole> roles, List<DiscordComponentEmoji> emoji)
		{
			var dtoOptions = roles.Select((opt, index) => new RoleMenuOptionDto()
			{
				RoleId = opt.Id,
				MessageId = message.Id,
				Description = $"Get or keep the {opt.Name} role",
				EmojiName = roles[index] is {} emj ? emj.Id is 0 ? emj.Name : emj.Id.ToString(CultureInfo.InvariantCulture) : null
			}).ToArray();

			var rmDTO = new RoleMenuDto()
			{
				MessageId = message.Id,
				Options = dtoOptions,
			};

			//await _mediator.Send(new CreateRoleMenuRequest(rmDTO));
		}

		private async Task<DiscordRole> GetRoleInputAsync(CommandContext ctx, DiscordMessage message, DiscordUser user, InteractivityExtension interactivity)
		{
			var contentRestore = message.Content;
			var componentRestore = message.Components;
			await message.ModifyAsync("What role would you like to add? Type cancel to cancel.");
			
			var result = await interactivity.WaitForMessageAsync(m => m.Author == user);

			if (string.Equals(result.Result.Content, "cancel", StringComparison.OrdinalIgnoreCase))
			{
				await RevertAsync();
				return null;
			}
			else
			{
				var roleParser = (IArgumentConverter<DiscordRole>)new DiscordRoleConverter();
				var res = await roleParser.ConvertAsync(result.Result.Content, ctx);

				if (!res.HasValue)
				{
					await RevertAsync();
					return null;
				}

				if (res.Value.Position >= ctx.Guild.CurrentMember.Roles.Last().Position)
				{
					await message.ModifyAsync("That role is too high! I can't give it out.");
					await Task.Delay(3000);
					await RevertAsync();
					return null;
				}
				await RevertAsync();
				return res.Value;
			}

			Task RevertAsync() => message.ModifyAsync(m => m
				.WithContent(contentRestore)
				.AddComponents(componentRestore.First().Components)
				.AddComponents(componentRestore.Last().Components));
		}
		
		
		private async Task<DiscordComponentEmoji> GetEmojiInputAsync(DiscordClient client, DiscordMessage message, DiscordUser user, InteractivityExtension interactivity)
		{
			var contentRestore = message.Content;
			var componentRestore = message.Components;
			await message.ModifyAsync("What emoji would you like to add? Type cancel to cancel.");

			var result = await interactivity.WaitForMessageAsync(m => m.Author == user);

			if (string.Equals(result.Result.Content, "cancel", StringComparison.OrdinalIgnoreCase))
			{
				await message.ModifyAsync(m => m
					.WithContent(contentRestore)
					.AddComponents(componentRestore.First().Components)
					.AddComponents(componentRestore.Last().Components));
				return null;
			}
			else
			{
				DiscordComponentEmoji emoji = null;
				
				if (DiscordEmoji.TryFromName(client, result.Result.Content, true, out var emj))
					emoji = new(emj);
				
				if (Regex.Match(result.Result.Content, @"^\<a?:\S:(\d+)\>") is { Success: true } match) 
					emoji = new(ulong.Parse(match.Value));
				
				var yesButton = new DiscordButtonComponent(ButtonStyle.Success, "y", "Yes");
				var noButton = new DiscordButtonComponent(ButtonStyle.Danger, "n", "No");

				await message.ModifyAsync(m => m.WithContent("Are you sure?").AddComponents(yesButton, noButton));

				var buttonResult = await message.WaitForButtonAsync(CancellationToken.None);
				await buttonResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
				
				
				if (buttonResult.Result.Id == yesButton.CustomId)
				{
					return emoji;
				}
				else
				{
					await message.ModifyAsync(m => m
						.WithContent(contentRestore)
						.AddComponents(componentRestore.First().Components)
						.AddComponents(componentRestore.Last().Components));
					return null;
				}
			}
		}
	}
}