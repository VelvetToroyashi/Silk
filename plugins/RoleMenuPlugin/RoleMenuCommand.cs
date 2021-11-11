using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using FuzzySharp;
using MediatR;
using RoleMenuPlugin.Database;

namespace RoleMenuPlugin
{
	/// <summary>
	/// The command module responsible for creating, modifying, and deleting role menus.
	/// </summary>
	[Group("rolemenu")]
	[Aliases("role-menu", "rm")]
	[Description("Role menu related commands.")]
	[RequirePermissions(Permissions.ManageRoles)]
	[ModuleLifespan(ModuleLifespan.Transient)]
	public sealed class RoleMenuCommand : BaseCommandModule
	{
		private readonly IMediator _mediator;
		public RoleMenuCommand(IMediator mediator) => _mediator = mediator;
		
		private static readonly DiscordButtonComponent _quitButton = new(ButtonStyle.Danger, "rm-quit", "Quit");
		private readonly DiscordButtonComponent _finishButton = new(ButtonStyle.Success, "rm-finish", "Finish", true);
			
		private readonly DiscordButtonComponent _editButton = new(ButtonStyle.Primary, "rm-edit", "Edit the current options");
		private readonly DiscordButtonComponent _addFullButton = new(ButtonStyle.Primary, "rm-add-full", "Add option (full)");
		private readonly DiscordButtonComponent _addRoleOnlyButton = new(ButtonStyle.Secondary, "rm-add", "Add option (role only)");
		
		private readonly DiscordButtonComponent _htuButton = new(ButtonStyle.Primary, "rm-htu", "How do I use this thing???");
		
		[Command]
		[Description("Create a new role-menu. \n\n" +
		             "**Disclaimer**:\n\n" +
		             "V2 of this command is currently considered beta software.\n" +
		             "It however, is generally considered stable, and has been shipped.\n" +
		             "If you experience any issues when creating a role menu, contact support.")]
		public async Task Create(CommandContext ctx, DiscordChannel? channel = null)
		{
			channel ??= ctx.Channel;
			
			if (!channel.PermissionsFor(ctx.Guild.CurrentMember).HasPermission(Permissions.SendMessages))
            {
                await ctx.RespondAsync("I don't have permission to send messages in that channel.");
                return;
            }

			DiscordMessage initialMenuMessage = await ctx.RespondAsync("Warming up...");
			
			InteractivityExtension interactivity = ctx.Client.GetInteractivity();

			string selectionId;

			var options = new List<RoleMenuOptionDto>();
			
			while (true)
			{ 
				ResetToMenu(ref initialMenuMessage);
				var selection = (await interactivity.WaitForButtonAsync(initialMenuMessage, ctx.User, CancellationToken.None)).Result;
				selectionId = selection.Id;

				var t = selectionId switch 
                {
                    "rm-quit" => Task.CompletedTask,
                    "rm-finish" => Task.CompletedTask,
                    "rm-edit" => Edit(ctx, selection.Interaction, initialMenuMessage, interactivity, options),
					"rm-add-full" => AddFull(ctx, selection.Interaction, initialMenuMessage, interactivity),
                    //"rm-add" => AddRoleOnly(ctx, channel),
                    "rm-htu" => ShowHelpAsync(selection.Interaction),
                    _ => Task.CompletedTask
                };

				if (t.IsCompleted)
					break;
				
				await t;

				if (t is Task<RoleMenuOptionDto> t2)
					options.Add(t2.Result);

				_ = options.Count > 1 ? _finishButton.Enable() : _finishButton.Disable();

				if (options.Count >= 25)
				{
					_addFullButton.Disable();
					_addRoleOnlyButton.Disable();
				}
				else
                {
                    _addFullButton.Enable();
                    _addRoleOnlyButton.Enable();
                }

				
				
			}

			if (selectionId == "rm-quit")
			{
				try
				{
					await ctx.Message.DeleteAsync();
				}
				catch
				{
					// ignored
				}

				await initialMenuMessage.DeleteAsync();
				
				return;
			}
			
			//TODO: Completion logic here? 
		}


		private async Task ShowHelpAsync(DiscordInteraction interaction)
		{
			await interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

			//write the help text using a string builder
			var sb = new StringBuilder();
			sb
			.AppendLine("**How to use this thing**")
			.AppendLine("There are a bombardment of options, and you may be curious as to what they do.")
			.AppendLine()
			.AppendLine("From left to right, I will explain what all the buttons are for.")
			.AppendLine("`Add option(full)`:")
			.Append("\u200b\t")
			.AppendLine("This option is the interactive way of adding roles, but can be a tad slow.")
			.Append("\u200b\t")
			.AppendLine("Using this button will prompt you for the role, an emoji to go with it, and the description.")
			.Append("\u200b\t")
			.AppendLine("For the role, it must not be `@everyone`, nor above either of our top roles. I can't assign those!")
			.Append("\u200b\t")
			.AppendLine("You can either mention the role directly, or type its name.")
			.Append("\u200b\t")
			.AppendLine("For the emoji, you can use any emoji, but they must be typed out properly.")
			.Append("\u200b\t")
			.AppendLine("(e.g. <a:catgiggle:853806288190439436> or 👋 and not catgiggle or \\:wave\\:)")
			.Append("\u200b\t")
			.AppendLine("Descriptions are also easy. They can be whatever you want, but they will limited to 100 characters.")
			.AppendLine()
			.AppendLine("`Add option(role only)`:")
			.Append("\u200b\t")
			.AppendLine("This is a faster, but more restricted way of adding roles.")
			.Append("\u200b\t")
			.AppendLine("You can only add the role, but you can add them in batches.")
			.Append("\u200b\t")
			.AppendLine("When using this option, you must mention the role directly (e.g. `@role`).")
			.Append("\u200b\t")
			.AppendLine("If you'd like to retro-actively add an emoji or description, you can use the edit button.")
			.Append("\u200b\t")
			.AppendLine("You can't add the `@everyone` role, nor above either of our top roles.")
			.AppendLine()
			.AppendLine("`Edit option`:")
			.Append("\u200b\t")
			.AppendLine("This one is somewhat self-explanatory, but it allows you to edit options you've added to the current role menu.")
			.AppendLine()
			.AppendLine("`Finish`:")
			.Append("\u200b\t")
			.AppendLine("This is the final button. It will send the role menu to the channel you specified.")
			.AppendLine()
			.AppendLine("`Quit`:")
			.Append("\u200b\t")
			.AppendLine("This will cancel the role menu and delete the message you started it with.")
			.AppendLine()
			.AppendLine("**Note**:")
			.Append("\u200b\t")
			.AppendLine("If you're not sure what to do, try the `Add option(full)` button first. It's a bit slower, but it's the easiest.")
			.Append("\u200b\t")
			.AppendLine("Also, this is considered beta software, so please report any bugs you find to the developer.");
			
			await interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().WithContent(sb.ToString()).AsEphemeral(true));
		}


		private static async Task<RoleMenuOptionDto?> AddFull(CommandContext ctx, DiscordInteraction interaction, DiscordMessage menuMessage, InteractivityExtension interactivity)
		{
			string current = "";
			DiscordRole role = null!;
			DiscordEmoji? emoji = null;
			string? description;

			await interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
			
			DiscordMessage tipMessage = await interaction.CreateFollowupMessageAsync(new() { Content = "\u200b", IsEphemeral = true});
			
			while (true)
			{
				await interaction.EditFollowupMessageAsync(tipMessage.Id, new DiscordWebhookBuilder()
					.WithContent("Enter the name of the role you want to add to the menu.\n" +
					             "If you don't see the role in the list, you may need to type it in the exact format it appears in the server (e.g. `@Role`).\n" +
					             "Type `cancel` to cancel adding roles."));
				
				var input = await GetInputWithContentAsync();

				if (input.Cancelled)
					return null;

				if (input.Value!.MentionedRoles.Count is not 0)
				{
					var r = input.Value.MentionedRoles[0];
					
					// Ensure the role is not above the user's highest role
					var valid = await ValidateRoleHeirarchyAsync(ctx, interaction, r, tipMessage);

					if (!valid)
						continue;
					
					role = r;
				}
				else
				{
					// Accurate route: Use DiscordRoleConverter | This is the most accurate way to get the role, but doesn't support names
					// Less accurate route: Use FuzzySharp to fuzzy match the role name, but use a high dropoff threshold

					//We need to check role names via RoleConverter casted to IArgumentConverter<DiscordRole>
					var roleConverter = (IArgumentConverter<DiscordRole>)new DiscordRoleConverter();

					//Try to convert the input to a role
					var result = await roleConverter.ConvertAsync(current, ctx);

					if (result.HasValue)
					{
						role = result.Value;
					}
					else
					{
						var fuzzyRes = Process.ExtractSorted((current, default(ulong)),
								ctx.Guild.Roles.Select(r => (r.Value.Name, r.Key)),
								r => r.Item1, cutoff: 90)
							.FirstOrDefault();

						if (fuzzyRes?.Value is null)
						{
							await interaction.EditFollowupMessageAsync(tipMessage.Id, new DiscordWebhookBuilder().WithContent("Could not find that role. Try again."));
							await Task.Delay(2200);
							continue;
						}

						role = ctx.Guild.Roles[fuzzyRes.Value.Item2];
					}

					var valid = await ValidateRoleHeirarchyAsync(ctx, interaction, role, tipMessage);
					
					if (!valid)
                        continue;
					
					role ??= result.Value;

					await input.Value.DeleteAsync();
					break;
				}
			}

			while (true)
			{
				await interaction.EditFollowupMessageAsync(tipMessage.Id, new DiscordWebhookBuilder()
                    .WithContent("Enter the emoji you want to use to represent this role.\n" +
                                 "If you don't see the emoji in the list, you may need to type it in the exact format it appears in the server (e.g. `:emoji:`).\n" +
                                 "Type `cancel` to cancel adding this role. Type `skip` to skip adding an emoji."));
				
				Result<DiscordMessage> input = await GetInputWithContentAsync();

				if (input.Cancelled)
					return null;

				if (input.Value is null)
					break;
				
				var converter = (IArgumentConverter<DiscordEmoji>)new DiscordEmojiConverter();
				
				var result = await converter.ConvertAsync(input.Value.Content, ctx);
				
				if (!result.HasValue)
                {
                    await interaction.EditFollowupMessageAsync(tipMessage.Id, new DiscordWebhookBuilder().WithContent("Could not find that emoji. Try again."));
                    await Task.Delay(2200);
                    continue;
                }
				await input.Value.DeleteAsync();
				
				emoji = result.Value;

				
				
				break;
			}

			while (true)
            {
	            await interaction.EditFollowupMessageAsync(tipMessage.Id, new DiscordWebhookBuilder().WithContent("Enter a description for this role.\n" +
																													"Descriptions will be truncated at 100 characters.\n" +
																													"Type `cancel` to cancel adding this role. Type `skip` to skip adding a description."));
	            
	            Result<DiscordMessage> input = await GetInputWithContentAsync();

                if (input.Cancelled)
                    return null;
                
                description = input.Value?.Content?.Length > 100 ? input.Value.Content[..100] : input.Value?.Content;

                if (input.Value is not null)
					await input.Value.DeleteAsync();

	            break;
            }
			
			bool confirm = await GetConfirmationAsync();
			
			if (!confirm)
                return null;

			return new()
			{
                RoleId = role.Id,
                RoleName = role.Name,
                EmojiName = emoji?.Name,
                Description = description,
                GuildId = ctx.Guild.Id,
            };

			async Task<bool> GetConfirmationAsync()
			{
				var confirmMessage = await interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
					.WithContent("Are you sure you want to add this role to the menu?\n" +
					             $"Role: {role.Name}\n" +
					             $"Emoji: {emoji}\n" +
					             $"Description: {description ?? "None"}")
					.AddComponents(
						new DiscordButtonComponent(ButtonStyle.Success, "y", "Yes"),
						new DiscordButtonComponent(ButtonStyle.Danger, "n", "No (Cancel)")
						)
					.AsEphemeral(true));

				var res = await interactivity.WaitForButtonAsync(confirmMessage, TimeSpan.FromMinutes(10));

				if (!res.TimedOut)
					await res.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
				
				return res.Result?.Id switch
                {
                    "y" => true,
                    "n" => false,
                    _ => false
                };
			}

			async Task<Result<DiscordMessage?>> GetInputWithContentAsync()
			{
				var inp = await GetInputAsync(interactivity, interaction, tipMessage.Id);

				if (inp.Value is not null)
					current = inp.Value.Content;

				return inp;
			}
		}
		
		private static async Task<bool> ValidateRoleHeirarchyAsync(CommandContext ctx, DiscordInteraction interaction, DiscordRole r, DiscordMessage tipMessage)
		{
			if (r.Position >= ctx.Guild.CurrentMember.Roles.Max(x => x.Position))
			{
				await interaction.EditFollowupMessageAsync(tipMessage.Id, new DiscordWebhookBuilder()
					.WithContent("You can't add roles that are above your highest role."));
				return false;
			}

			if (r.Position >= ctx.Guild.CurrentMember.Roles.Max(x => x.Position))
			{
				await interaction.EditFollowupMessageAsync(tipMessage.Id, new DiscordWebhookBuilder()
					.WithContent("I cannot assign that role as it's above my highest role."));

				await Task.Delay(2200);
				return false;
			}

			if (r == ctx.Guild.EveryoneRole)
			{
				await interaction.EditFollowupMessageAsync(tipMessage.Id, new DiscordWebhookBuilder()
					.WithContent("I cannot assign the everyone role as it's special and cannot be assigned."));

				await Task.Delay(2200);
				return false;
			}

			if (r.IsManaged)
			{
				await interaction.EditFollowupMessageAsync(tipMessage.Id, new DiscordWebhookBuilder()
					.WithContent("I cannot assign that role as it's managed and cannot be assigned."));

				await Task.Delay(2200);
				return false;
			}

			return true;
		}

		private static async Task Edit(CommandContext ctx, DiscordInteraction interaction, DiscordMessage initialMenuMessage, InteractivityExtension interactivity, List<RoleMenuOptionDto> options)
		{
			await interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

			var sopts = options.Select((x, i) =>
				new DiscordSelectComponentOption(x.RoleName, i.ToString(), x.Description));

			var selectionMessage = await interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AsEphemeral(true)
				.AddComponents(new DiscordSelectComponent("rm-edit-current", "Select the option you want to edit", sopts))
				.AddComponents(_quitButton));

			var res = await interactivity
				.WaitForEventArgsAsync<ComponentInteractionCreateEventArgs>(c => c.Message == selectionMessage, TimeSpan.FromMinutes(5));

			if (res.TimedOut || res.Result.Id == "rm-quit")
            {
                await interaction.EditFollowupMessageAsync(selectionMessage.Id, new DiscordWebhookBuilder().WithContent("Cancelled."));
                return;
            }
			
			var option = options[int.Parse(res.Result.Values[0])]; // Default min value is 1, so there's always an index.
			
			var changeRoleButton = new DiscordButtonComponent(ButtonStyle.Primary, "rm-change-role", "Change Role");
			var changeEmojiButton = new DiscordButtonComponent(ButtonStyle.Secondary, "rm-change-emoji", "Change Emoji");
			var changeDescriptionButton = new DiscordButtonComponent(ButtonStyle.Secondary, "rm-change-description", "Change Description");
			var deleteButton = new DiscordButtonComponent(ButtonStyle.Danger, "rm-delete", "Delete");
			
			var addEmojiButton = new DiscordButtonComponent(ButtonStyle.Success, "rm-add-emoji", "Add Emoji");
			var addDescriptionButton = new DiscordButtonComponent(ButtonStyle.Success, "rm-add-description", "Add Description");
			
			await res.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder()
				.WithContent($"Editing option {res.Result.Values[0]}")
				.AddEmbed(new DiscordEmbedBuilder()
					.WithColor(DiscordColor.Wheat)
					.WithTitle("Current menu option:")
					.AddField("Role", option.RoleName, true)
					.AddField("Emoji", option.EmojiName is null ? "Not set." : ulong.TryParse(option.EmojiName, out var emoji) ? $"<a:{emoji}>" : option.EmojiName, true)
					.AddField("Description", option.Description ?? "None"))
				.AddComponents(changeRoleButton, option.EmojiName is null ? addEmojiButton : changeEmojiButton, option.Description is null ? addDescriptionButton : changeDescriptionButton, deleteButton, _quitButton));

			selectionMessage = await res.Result.Interaction.GetOriginalResponseAsync();
			//TODO: Make this a loop.
			//TODO: Add buttons to make an option mutually exclusive.

			res = await interactivity.WaitForButtonAsync(selectionMessage);
			
			if (res.TimedOut || res.Result.Id == "rm-quit")
            {
                await interaction.EditFollowupMessageAsync(selectionMessage.Id, new DiscordWebhookBuilder().WithContent("Cancelled."));
                return;
            }

			var t = res.Result.Id switch
			{
				"rm-change-role" => ChangeRoleAsync(),
				"rm-change-emoji" => ChangeEmojiAsync(),
				"rm-change-description" => ChangeDescriptionAsync(),
				"rm-delete" => DeleteAsync(),
				"rm-add-emoji" => AddEmojiAsync(),
				"rm-add-description" => AddDescriptionAsync(),
				_ => throw new ArgumentException("Invalid button id.")
			};

			await res.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
			
			await t;

			async Task ChangeRoleAsync()
			{
				while (true)
				{
					var input = await GetInputAsync(interactivity, interaction, selectionMessage.Id);

					if (input.Cancelled)
						return;

					DiscordRole r;
					if (input.Value!.MentionedRoles.Count is not 0)
					{
						r = input.Value.MentionedRoles[0];

						// Ensure the role is not above the user's highest role
						var valid = await ValidateRoleHeirarchyAsync(ctx, interaction, r, selectionMessage);

						if (!valid)
							continue;

						option = option with
						{
							RoleId = r.Id,
							RoleName = r.Name
						};
					}
					else
					{
						// Accurate route: Use DiscordRoleConverter | This is the most accurate way to get the role, but doesn't support names
						// Less accurate route: Use FuzzySharp to fuzzy match the role name, but use a high dropoff threshold

						//We need to check role names via RoleConverter casted to IArgumentConverter<DiscordRole>
						var roleConverter = (IArgumentConverter<DiscordRole>)new DiscordRoleConverter();

						//Try to convert the input to a role
						var result = await roleConverter.ConvertAsync(input.Value.Content, ctx);

						if (result.HasValue)
						{
							r = result.Value;
						}
						else
						{
							var fuzzyRes = Process.ExtractSorted((input.Value.Content, default(ulong)),
									ctx.Guild.Roles.Select(r => (r.Value.Name, r.Key)),
									r => r.Item1, cutoff: 90)
								.FirstOrDefault();

							if (fuzzyRes?.Value is null)
							{
								await interaction.EditFollowupMessageAsync(selectionMessage.Id, new DiscordWebhookBuilder().WithContent("Could not find that role. Try again."));
								await Task.Delay(2200);
								continue;
							}

							r = ctx.Guild.Roles[fuzzyRes.Value.Item2];
						}

						var valid = await ValidateRoleHeirarchyAsync(ctx, interaction, r, selectionMessage);

						if (!valid)
							continue;
						
						option = option with
						{
							RoleId = r.Id,
                            RoleName = r.Name
                        };
					}
				}
			}
			
			async Task ChangeEmojiAsync()
            {
                
            }
			
			async Task ChangeDescriptionAsync()
            {
	            
	            
            }
			
			Task DeleteAsync()
			{
				options.Remove(option);
				return Task.CompletedTask;
			}
			
			async Task AddEmojiAsync()
            {
                
            }
			
			async Task AddDescriptionAsync()
            {
                
            }
			
		}
		
		private static async Task<Result<DiscordMessage?>> GetInputAsync(InteractivityExtension it, DiscordInteraction interaction, ulong message)
		{
			var input = await it.WaitForMessageAsync(m => m.Author == interaction.User, TimeSpan.FromMinutes(14));
			
			if (input.TimedOut)
				return new(true, null);
				
			if (input.Result.Content == "cancel")
			{
				await interaction.EditFollowupMessageAsync(message, new DiscordWebhookBuilder().WithContent("Cancelled."));

				await input.Result.DeleteAsync();

				return new(true, null);
			}
				
			if (input.Result.Content == "skip")
			{
				await interaction.EditFollowupMessageAsync(message, new DiscordWebhookBuilder().WithContent("Skipped."));

				await input.Result.DeleteAsync();

				return new(true, null);
			}
				
		
			return new(false, input.Result);
		}
		
		private void ResetToMenu(ref DiscordMessage message)
		{
			message = message.ModifyAsync(m => m
				.WithContent($"Silk! Role Menu Creator v2.0")
				.AddComponents(_addFullButton, _addRoleOnlyButton, _editButton)
				.AddComponents(_finishButton, _quitButton, _htuButton))
				.GetAwaiter().GetResult();
		}
		
		
		
		private record Result<T>(bool Cancelled, T? Value);
        
	}
}