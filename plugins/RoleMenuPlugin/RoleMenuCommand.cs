using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
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
			
			while (true)
			{ 
				ResetToMenu(ref initialMenuMessage);
				var selection = (await interactivity.WaitForButtonAsync(initialMenuMessage, ctx.User, CancellationToken.None)).Result;
				selectionId = selection.Id;

				var t = selectionId switch 
                {
                    "rm-quit" => Task.CompletedTask,
                    "rm-finish" => Task.CompletedTask,
                    "rm-edit" => Edit(ctx, channel),
					"rm-add-full" => AddFull(ctx, selection.Interaction, initialMenuMessage, interactivity),
                    //"rm-add" => AddRoleOnly(ctx, channel),
                    //"rm-htu" => HelpText(ctx, channel),
                    _ => Task.CompletedTask
                };

				if (t.IsCompleted)
					break;
				
				await t;
			}

			if (selectionId == "rm-quit")
                return;
			
			//TODO: Completion logic here?
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
				
				Result<DiscordMessage> input = await GetInputAsync();

				if (input.Cancelled)
					return null;

				if (input.Value!.MentionedRoles.Count is 0)
				{
					// Accurate route: Use DiscordRoleConverter | This is the most accurate way to get the role, but doesn't support names
					// Less accurate route: Use FuzzySharp to fuzzy match the role name, but use a high dropoff threshold
				
					//We need to check role names via RoleConverter casted to IArgumentConverter<DiscordRole>
					var roleConverter = (IArgumentConverter<DiscordRole>)new DiscordRoleConverter();
				
					//Try to convert the input to a role
					var result = await roleConverter.ConvertAsync(current, ctx);

					if (!result.HasValue)
					{
						var fuzzyRes = Process.ExtractSorted((current, default(ulong)),
							ctx.Guild.Roles.Select(r => (r.Value.Name, r.Key)),
							r => r.Item1, cutoff: 90).FirstOrDefault();

						if (fuzzyRes?.Value is null)
						{
							await interaction.EditFollowupMessageAsync(tipMessage.Id, new DiscordWebhookBuilder().WithContent("Could not find that role. Try again."));
							await Task.Delay(2200);
							continue;
						}

						role = ctx.Guild.Roles[fuzzyRes.Value.Item2];
					}
				
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
				
				Result<DiscordMessage> input = await GetInputAsync();

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
				
				emoji = result.Value;

				await input.Value.DeleteAsync();
				
				break;
			}

			while (true)
            {
	            await interaction.EditFollowupMessageAsync(tipMessage.Id, new DiscordWebhookBuilder().WithContent("Enter a description for this role.\n" +
																													"Descriptions will be truncated at 100 characters.\n" +
																													"Type `cancel` to cancel adding this role. Type `skip` to skip adding a description."));
	            
	            Result<DiscordMessage> input = await GetInputAsync();

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
						new DiscordButtonComponent(ButtonStyle.Success, "Yes", "y"),
						new DiscordButtonComponent(ButtonStyle.Danger, "No (cancel)", "n")
						));

				var res = await interactivity.WaitForButtonAsync(confirmMessage, TimeSpan.FromMinutes(10));

				return res.Result?.Id switch
                {
                    "y" => true,
                    "n" => false,
                    _ => false
                };
			}

			async Task<Result<DiscordMessage>> GetInputAsync()
			{
				var input = await interactivity.WaitForMessageAsync(m => m.Author == interaction.User,  TimeSpan.FromMinutes(14));

				if (input.TimedOut)
					return new(true, null);
				
				if (input.Result.Content == "cancel")
				{
					await interaction.EditFollowupMessageAsync(tipMessage.Id, new DiscordWebhookBuilder().WithContent("Cancelled."));

					await input.Result.DeleteAsync();

					return new(true, null);
				}
				
				if (input.Result.Content == "skip")
                {
                    await interaction.EditFollowupMessageAsync(tipMessage.Id, new DiscordWebhookBuilder().WithContent("Skipped."));

                    await input.Result.DeleteAsync();

                    return new(false, null);
                }
				
				current = input.Result.Content;
				return new(false, input.Result);
			}
		}

		private async Task Edit(CommandContext ctx, DiscordChannel channel)
		{
				
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