using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using MediatR;
using RoleMenuPlugin.Database;
using RoleMenuPlugin.Database.MediatR;

namespace RoleMenuPlugin
{
	/// <summary>
	/// The command module responsible for creating, modifying, and deleting role menus.
	/// </summary>
	[Group("rolemenu")]
	[Aliases("role-menu", "rm")]
	[Description("Role menu related commands.")]
	[RequirePermissions(Permissions.ManageRoles)]
	public sealed class RoleMenuCommand : BaseCommandModule
	{
		private readonly IMediator _mediator;
		public RoleMenuCommand(IMediator mediator) => _mediator = mediator;
		
		private readonly DiscordButtonComponent quitButton = new(ButtonStyle.Danger, "rm-quit", "Quit");
		private readonly DiscordButtonComponent finishButton = new(ButtonStyle.Success, "rm-finish", "Finish", true);
			
		private readonly DiscordButtonComponent editButton = new(ButtonStyle.Primary, "rm-edit", "Edit an existing role-menu");
		private readonly DiscordButtonComponent addFullButton = new(ButtonStyle.Primary, "rm-add-full", "Add option (full)");
		private readonly DiscordButtonComponent addRoleOnlyButton = new(ButtonStyle.Secondary, "rm-add", "Add option (role only)");
		

		[Command]
		[Description("Creates a new role menu in the specified channel.")]
		public async Task Create(CommandContext ctx, DiscordChannel channel)
		{
			if (!channel.PermissionsFor(ctx.Guild.CurrentMember).HasPermission(Permissions.SendMessages))
			{
				await ctx.RespondAsync("Sorry, but I can't send messages to that channel! Try a different one!");
				return;
			}

			var interactivity = ctx.Client.GetInteractivity();
			var rmoOptions = new List<RoleMenuOptionDto>();

			var message = await ctx.Channel.SendMessageAsync("Role Menu Setup:");

			while (true)
			{
				if (rmoOptions.Count >= 25)
				{
					addFullButton.Disable();
					addRoleOnlyButton.Disable();
				}
				
				if (rmoOptions.Count > 1) 
					finishButton.Enable();

				message = await message.ModifyAsync(m =>
					m.WithContent("Role menu setup:")
						.AddComponents(addFullButton, addRoleOnlyButton, editButton)
						.AddComponents(finishButton, quitButton));
				
				var selection = await interactivity.WaitForButtonAsync(message, ctx.User,  CancellationToken.None);
				await selection.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

				if (selection.Result.Id == "rm-quit")
				{
					await selection.Result.Interaction.DeleteOriginalResponseAsync();
					return;
				}

				if (selection.Result.Id == "rm-finish")
				{
					await selection.Result.Interaction.DeleteOriginalResponseAsync();
					break;
				}

				var task = selection.Result.Id switch
				{
					"rm-add-full" => AddFull(),
					"rm-add" => AddRoleOnly(),
					"rm-edit" => selection.Result.Interaction.CreateFollowupMessageAsync(new() { Content = "Coming soon:tm:", IsEphemeral = true}),
					_ => Task.CompletedTask,
				};

				await task;

				async Task AddFull()
				{
					var option = new RoleMenuOptionDto();
					
					//Role
					await message.ModifyAsync(m => m.WithContent("Role:"));
					var role = await interactivity.WaitForMessageAsync(m => m.Author == ctx.User && m.MentionedRoles.Count > 0);
					option = option with { RoleId = role.Result.MentionedRoles[0].Id };
					await role.Result.DeleteAsync();
					
					//Emoji
					await message.ModifyAsync(m => m.WithContent("Emoji (type skip to skip)"));
					
					GetEmoji:
					var emoji = await interactivity.WaitForMessageAsync(m => m.Author == ctx.User);
					
					var econ = (IArgumentConverter<DiscordEmoji>)new DiscordEmojiConverter();
					var compEmoji = await econ.ConvertAsync(emoji.Result.Content, ctx);
					
					if (!string.Equals(emoji.Result.Content, "skip", StringComparison.OrdinalIgnoreCase) && !compEmoji.HasValue)
						goto GetEmoji;

					if (compEmoji.HasValue)
						option = option with { EmojiName = compEmoji.Value.Id is 0 ? compEmoji.Value.Name : compEmoji.Value.Id.ToString(CultureInfo.InvariantCulture) };

					await emoji.Result.DeleteAsync();
					//Description
					await message.ModifyAsync(m => m.WithContent("Role description (trunctated at 100 characters, type skip to skip)"));
					
					GetDescription:
					var description = await interactivity.WaitForMessageAsync(m => m.Author == ctx.User);

					if (string.Equals(description.Result.Content, "skip", StringComparison.OrdinalIgnoreCase))
						return;
					
					var confirm = new DiscordButtonComponent(ButtonStyle.Success, "rm-confirm", "Yes");
					var decline =  new DiscordButtonComponent(ButtonStyle.Danger, "rm-decline", "No");

					await message.ModifyAsync(m => m.WithContent("Are you sure?").AddComponents(confirm, decline));

					var confirmation = await message.WaitForButtonAsync(ctx.User, CancellationToken.None);
					await confirmation.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
					
					if (confirmation.Result.Id == decline.CustomId)
						return;
					
					rmoOptions.Add(option);
				}

				async Task AddRoleOnly()
				{
					var msg = await selection.Result.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
						.WithContent("You can add multiple roles at once!")
						.AddComponents(quitButton)
						.AsEphemeral(true));
					
					var cancalInput = interactivity.WaitForButtonAsync(msg);
					var messageInput = interactivity.WaitForMessageAsync(m => m.Author == ctx.User && m.MentionedRoles.Count > 0);

					await Task.WhenAny(cancalInput, messageInput);

					if (cancalInput.IsCompleted)
					{
						await cancalInput.Result.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
						return;
					}
					
					var msgInput = messageInput.Result.Result; // This is OK; the task is already completed.
					var roles = msgInput.MentionedRoles.AsEnumerable();
					
					var totalCount = rmoOptions.Count + roles.Count();
					if (totalCount > 25)
					{
						await selection.Result.Interaction.CreateFollowupMessageAsync(new()
						{
							Content = $"⚠ You're attempting to add {roles.Count()} roles, but there's only {25 - rmoOptions.Count} available slots left. \n" +
							          $"{totalCount - 25} roles will be ignored. \n" +
							          $"Ignored:\n{string.Join('\n', roles.TakeLast(totalCount - 25).Select(m => m.Mention))}"
							          
						});
						roles = roles.Except(roles.TakeLast(totalCount - 25));
					}
					
					rmoOptions.AddRange(roles.Select(r => new RoleMenuOptionDto() { RoleId = r.Id }));

					await selection.Result.Interaction.CreateFollowupMessageAsync(new() { Content = "Done.", IsEphemeral = true });
				}
			}

			var msg = await channel.SendMessageAsync(new DiscordMessageBuilder()
				.WithContent("**Role Menu**. Use the button below to get roles.")
				.AddComponents(new DiscordButtonComponent(ButtonStyle.Primary, RoleMenuRoleService.RoleMenuPrefix, "Get roles")));

			await _mediator.Send(new CreateRoleMenuRequest(new RoleMenuDto() { MessageId = msg.Id, Options = rmoOptions }));
		}
	}
}