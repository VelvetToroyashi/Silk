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
		
		private readonly DiscordButtonComponent _quitButton = new(ButtonStyle.Danger, "rm-quit", "Quit");
		private readonly DiscordButtonComponent _finishButton = new(ButtonStyle.Success, "rm-finish", "Finish", true);
			
		private readonly DiscordButtonComponent _editButton = new(ButtonStyle.Primary, "rm-edit", "Edit the current options");
		private readonly DiscordButtonComponent _addFullButton = new(ButtonStyle.Primary, "rm-add-full", "Add option (full)");
		private readonly DiscordButtonComponent _addRoleOnlyButton = new(ButtonStyle.Secondary, "rm-add", "Add option (role only)");
		

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
			var quitButton = new DiscordButtonComponent(_quitButton);
			
			while (true)
			{
				if (rmoOptions.Count >= 25)
				{
					_addFullButton.Disable();
					_addRoleOnlyButton.Disable();
				}
				
				if (rmoOptions.Count > 1) 
					_finishButton.Enable();

				message = await message.ModifyAsync(m =>
					m.WithContent("Role menu setup:")
						.AddComponents(_addFullButton, _addRoleOnlyButton, _editButton)
						.AddComponents(_finishButton, quitButton));
				
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
					"rm-edit" => Edit(),
					_ => Task.CompletedTask,
				};

				await task;

			async Task Edit()
			{
				if (!rmoOptions.Any())
				{
					await selection.Result.Interaction.CreateFollowupMessageAsync(new() { Content = "You don't have any options yet!" });
					return;
				}

				var options = new List<DiscordSelectComponentOption>();

				for (var i = 0; i < rmoOptions.Count; i++)
				{
					var opt = rmoOptions[i];
					options.Add(new(opt.RoleName, i.ToString(), opt.Description));
				}

				var dropdown = new DiscordSelectComponent("rm-edit", null, options);
				
				message = await selection.Result.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
					.WithContent("Please select the role you'd like to edit.")
					.AddComponents(dropdown));

				selection = await interactivity.WaitForSelectAsync(message, c => c.Id == "rm-edit" && c.User == ctx.User, CancellationToken.None);

				var index = int.Parse(selection.Result.Values[0]);
				var option = rmoOptions[index];
				
				var removeButton = new DiscordButtonComponent(ButtonStyle.Danger, "rm-remove-option", "Remove this role");
				
				var descriptionButton = new DiscordButtonComponent(
					style: option.Description is null ? ButtonStyle.Primary : ButtonStyle.Success,
					customId: "rm-edit-option-description",
					label: option.Description is null ? "Add a description" : "Change the description");

				var emojiButton = new DiscordButtonComponent(
					style: option.EmojiName is null ? ButtonStyle.Primary : ButtonStyle.Success,
					customId: "rm-edit-option-emoji",
					label: option.EmojiName is null ? "Add an emoji" : "Change the emoji on this option");
				
				var roleButton = new DiscordButtonComponent(ButtonStyle.Primary, "rm-edit-option-role", "Swap this role");

				var cancelButton = new DiscordButtonComponent(ButtonStyle.Secondary, "rm-cancel", "Cancel");

				await selection.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder()
					.WithContent("What would you like to do?")
					.AddComponents(removeButton, descriptionButton, emojiButton, roleButton, cancelButton));

				message = await selection.Result.Interaction.GetOriginalResponseAsync();

				selection = await interactivity.WaitForButtonAsync(message, ctx.User, CancellationToken.None);

				if (selection.Result.Id == "rm-cancel")
					return;

				var task = selection.Result.Id switch
				{
					"rm-remove-option" => RemoveRoleMenuOptionAsync(),
					"rm-edit-option-description" => AskForDescriptionAsync(),
					"rm-edit-option-emoji" => AskForEmojiAsync(),
					"rm-edit-option-role" => AskForRoleAsync(),
					_ => Task.CompletedTask
				};

				await selection.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
				
				await task;


					async Task RemoveRoleMenuOptionAsync()
					{
						rmoOptions.Remove(option);

						await selection.Result.Interaction.CreateFollowupMessageAsync(new() { Content = "Success.", IsEphemeral = true });
					}

					async Task AskForDescriptionAsync()
					{
						var descInput = await selection.Result.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
							.WithContent("What's the description of this role? (Cut off at 100 characters!)")
							.AsEphemeral(true)
							.AddComponents(new DiscordButtonComponent(ButtonStyle.Danger, "rm-quit", "Cancel")));

						using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(14));
						
						Wait:
						var msgInput = interactivity.WaitForMessageAsync(m => m.Author == ctx.User, TimeSpan.FromMinutes(5));
						var btnInput = interactivity.WaitForButtonAsync(descInput, ctx.User, cts.Token);

						await Task.WhenAny(msgInput, btnInput);

						if (!btnInput.IsCompleted && msgInput.IsCompleted)
						{
							var confirmation = await selection.Result.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
								.WithContent("Are you sure?")
								.AsEphemeral(true)
								.AddComponents(new DiscordButtonComponent(ButtonStyle.Success, "rm-agree", "Yes"), new DiscordButtonComponent(ButtonStyle.Danger, "rm-decline", "No")));

							var confirmed = await interactivity.WaitForButtonAsync(confirmation, ctx.User, cts.Token);

							if (confirmed.TimedOut)
								return;
							
							if (confirmed.Result.Id == "rm-decline")
								goto Wait;

							cts.Cancel();
							var description = msgInput.Result.Result.Content; // Task.WhenAny() guaruntees the task is completed. //

							if (description.Length > 100)
								description = description[..100].TrimEnd();
							option = option with { Description = description };

							rmoOptions[index] = option;

							await selection.Result.Interaction.EditFollowupMessageAsync(descInput.Id, new() { Content = "Success!" });
						}
					}

				async Task AskForEmojiAsync()
				{
					var emojiInput = await selection.Result.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
						.WithContent("What emoji will go with this option? It can be any emoji. \n" +
						             "Please provide a custom or unicode emoji.\n" +
						             "Example: <:catdrool:786419793811996673> OR :smile:")
						.AsEphemeral(true)
						.AddComponents(new DiscordButtonComponent(ButtonStyle.Danger, "rm-quit", "Cancel")));
					
					var cts = new CancellationTokenSource(TimeSpan.FromMinutes(14)); // Cut it short to account for elapsed time. //
					
					Wait:
					var msgInput = interactivity.WaitForMessageAsync(m => m.Author == ctx.User);
					var btnInput = interactivity.WaitForButtonAsync(emojiInput, ctx.User, cts.Token);

					await Task.WhenAny(msgInput, btnInput);

					if (!btnInput.IsCompleted)
					{
						var emoji = msgInput.Result.Result.Content; // Task.WhenAny() guaruntees the task is completed. //

						var parser = (IArgumentConverter<DiscordEmoji>)new DiscordEmojiConverter();

						var parseResult = await parser.ConvertAsync(emoji, ctx);

						if (!parseResult.HasValue)
						{
							await selection.Result.Interaction.CreateFollowupMessageAsync(new() { Content = "That doesn't appear to be an emoji! Try again!", IsEphemeral = true });
							goto Wait;
						}
						
						cts.Cancel();
						option = option with { EmojiName = parseResult.Value.ToString() };

						rmoOptions[index] = option;

						await selection.Result.Interaction.EditFollowupMessageAsync(emojiInput.Id, new() { Content = "Success!" });
					}
				}

				async Task AskForRoleAsync()
				{
					var roleInput = await selection.Result.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
						.WithContent("What role would you like to add?")
						.AsEphemeral(true)
						.AddComponents(new DiscordButtonComponent(ButtonStyle.Danger, "rm-quit", "Quit")));
					
					var cts = new CancellationTokenSource(TimeSpan.FromMinutes(14)); // Cut it short to account for elapsed time. //
					
					Wait:
					var msgInput = interactivity.WaitForMessageAsync(m => m.Author == ctx.User && m.MentionedRoles.Any());
					var btnInput = interactivity.WaitForButtonAsync(roleInput, ctx.User, cts.Token);

					await Task.WhenAny(msgInput, btnInput);

					if (!btnInput.IsCompleted)
					{
						var role = msgInput.Result.Result.MentionedRoles[0];

						if (role.Id == option.RoleId)
						{
							await selection.Result.Interaction.CreateFollowupMessageAsync(new() { Content = "That's the same role! You have to use a different one.", IsEphemeral = true });
							goto Wait;
						}
						
						cts.Cancel();
						option = option with { RoleId = role.Id, RoleName = role.Name };

						rmoOptions[index] = option;

						await selection.Result.Interaction.EditFollowupMessageAsync(roleInput.Id, new() { Content = "Success!" });
					}
				}
			}

				async Task AddFull()
				{
					var option = new RoleMenuOptionDto();
					
					//Role
					await message.ModifyAsync(m => m.WithContent("Role:"));
					var role = await interactivity.WaitForMessageAsync(m => m.Author == ctx.User && m.MentionedRoles.Count > 0);
					option = option with { RoleName = role.Result.MentionedRoles[0].Name, RoleId = role.Result.MentionedRoles[0].Id };
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
					
					var description = await interactivity.WaitForMessageAsync(m => m.Author == ctx.User);

					if (string.Equals(description.Result.Content, "skip", StringComparison.OrdinalIgnoreCase))
						return;

					var descriptionString = description.Result.Content;
					option = option with { Description = descriptionString.Length > 100 ? descriptionString[..100] : descriptionString };
					
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
						.AddComponents(_quitButton)
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
					
					rmoOptions.AddRange(roles.Select(r => new RoleMenuOptionDto
						{ RoleName = r.Name, RoleId = r.Id }));

					await selection.Result.Interaction.CreateFollowupMessageAsync(new() { Content = "Done.", IsEphemeral = true });
				}
			}

			var msg = await channel.SendMessageAsync(new DiscordMessageBuilder()
				.WithContent("**Role Menu**. Use the button below to get roles.")
				.AddComponents(new DiscordButtonComponent(ButtonStyle.Primary, RoleMenuRoleService.RoleMenuPrefix, "Get roles")));

			await _mediator.Send(new CreateRoleMenu.Request(new RoleMenuDto
				{ GuildId = ctx.Guild.Id, MessageId = msg.Id, Options = rmoOptions }));
		}
	}
}