using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Results;
using Remora.Results;
using RoleMenuPlugin.Database;
using Silk.Interactivity;

namespace RoleMenuPlugin
{
	/// <summary>
	/// The command module responsible for creating, modifying, and deleting role menus.
	/// </summary>
	[Group("rolemenu")]
	[Description("Role menu related commands.")]
	public sealed class RoleMenuCommand : CommandGroup
	{
		public class CreateCommand : CommandGroup
		{
			private const int MessageReadDelay = 3200; // The time, in ms to wait before editing messasges.
			
			private readonly ButtonComponent _addMenuInteractiveButton = new(ButtonComponentStyle.Primary,     "Add (Interactive)", CustomID: "rm-add-interactive");
			private readonly ButtonComponent _addMenuSimpleButton      = new (ButtonComponentStyle.Secondary,  "Add (Simple)",      CustomID: "rm-add-role-only");
			private readonly ButtonComponent _addMenuEditButton        = new (ButtonComponentStyle.Secondary,  "Edit Option",       CustomID: "rm-edit-options", IsDisabled: true);
    
			private readonly ButtonComponent _addMenuHelpButton   = new(ButtonComponentStyle.Primary,   "Help",      CustomID: "rm-help");
			private readonly ButtonComponent _addMenuFinishButton = new(ButtonComponentStyle.Success,     "Finish",    CustomID: "rm-finish", IsDisabled: true);
			private readonly ButtonComponent _addMenuCancelButton = new(ButtonComponentStyle.Danger,      "Cancel",    CustomID: "rm-cancel");
			
			private readonly MessageContext             _context;
			private readonly IDiscordRestUserAPI        _users;
			private readonly IDiscordRestChannelAPI     _channels;
			private readonly IDiscordRestGuildAPI       _guilds;
			private readonly InteractivityExtension     _interactivity;
			private readonly ILogger<RoleMenuCommand>   _logger;
			private readonly IDiscordRestInteractionAPI _interactions;
			
			private readonly List<RoleMenuOptionModel> _options = new(25);
			
			public CreateCommand
			(
				MessageContext             context,
				IDiscordRestUserAPI        users,
				IDiscordRestChannelAPI     channels,
				IDiscordRestGuildAPI       guilds,
				InteractivityExtension     interactivity,
				ILogger<RoleMenuCommand>   logger,
				IDiscordRestInteractionAPI interactions
			)
			{
				_context       = context;
				_users         = users;
				_channels      = channels;
				_guilds        = guilds;
				_interactivity = interactivity;
				_logger        = logger;
				_interactions  = interactions;
			}

			[Command("create")]
			[RequireDiscordPermission(DiscordPermission.ManageChannels)]
			public async Task<IResult> CreateAsync
			(
				[Description("The channel the role menu will be created in.\n" +
				             "This channel must be a text channel, and must allow sending messages.")]
				IChannel? channel = null
			)
			{
				if (channel is null)
				{
					var currentChannelResult = await _channels.GetChannelAsync(_context.ChannelID);

					if (currentChannelResult.IsSuccess)
					{
						channel = currentChannelResult.Entity;
					}
					else
					{
						//_logger.LogError("User appears to be in an invalid channel: {UserID}, {ChannelID}", _context.User.ID _context.ChannelID);
						return currentChannelResult;
					}
				}

				var channelValidationResult = await EnsureChannelPermissionsAsync(channel);

				if (!channelValidationResult.IsSuccess)
				{
					if (channelValidationResult.Error is not PermissionDeniedError)
						return channelValidationResult;

					return await _channels.CreateMessageAsync(_context.ChannelID, "Sorry, but I can't send messages to that channel!");
				}

				var messageResult = await _channels.CreateMessageAsync
					(
					 _context.ChannelID, "Silk! RoleMenu Creator V3",
				     components: new IMessageComponent[]
				     {
					     new ActionRowComponent(new IMessageComponent[]
					     {
						     _addMenuInteractiveButton,
						     _addMenuSimpleButton,
						     _addMenuEditButton,
					     }),
					     new ActionRowComponent(new IMessageComponent[]
					     {
						     _addMenuHelpButton,
						     _addMenuFinishButton,
						     _addMenuCancelButton,
					     })
				     }
					);

				if (!messageResult.IsSuccess)
					return await InformUserOfChannelErrorAsync();

				return await MenuLoopAsync(messageResult.Entity);
			}

			private async Task<IResult> MenuLoopAsync(IMessage message)
			{
				while (true)
				{
					var selectionResult = await _interactivity.WaitForButtonAsync(_context.User, message, this.CancellationToken);
					
					if (!selectionResult.IsSuccess || !selectionResult.IsDefined(out var selection))
					{
						await _channels.DeleteMessageAsync(_context.ChannelID, _context.MessageID);
						await _channels.EditMessageAsync(_context.ChannelID, message.ID, "Cancelled!", components: Array.Empty<IMessageComponent>());
						return Result.FromSuccess(); // TODO: Return a proper error
					}
					
					// We set the timeout to 14 minutes to ensure we can still use the interaction to update our message.
					var cts   = new CancellationTokenSource(TimeSpan.FromMinutes(14));
					var token = cts.Token;
					
					//This is safe to do because the predicate ensures this information is present before returning a result.
					var t = selection.Data.Value.CustomID.Value switch
					{
						"rm-add-interactive" => await CreateInteractiveAsync(selection, token),
						//"rm-simple"      => await CreateSimpleAsync(message, selection, token),
						//"rm-edit"        => await EditAsync(message, selection, token),
						//"rm-help"		   => Task.CompletedTask, // Ignored, handled in a handler.
						//"rm-finish"      => await FinishAsync(message, selection, token) 
						//"rm-cancel"
						_ => Result.FromSuccess() // An exception should be thrown here, as it's outside what should be possible.
					};
					
					await ShowMainMenuAsync(selection, _options.Count);
				}

				return Result.FromSuccess();
			}
			
			private async Task<IResult> CreateInteractiveAsync(IInteraction interaction, CancellationToken ct)
			{
				const string DescriptionInputMessage = "What role would you like for the role menu?\n" +
				                                       "Type `cancel` to cancel. (Press the help button if you're stuck!)";
				
				await _interactions.CreateInteractionResponseAsync
					(
					 interaction.ID,
					 interaction.Token,
					 new InteractionResponse
						(
                         InteractionCallbackType.ChannelMessageWithSource,
                         new InteractionCallbackData
	                         (
	                          Content: DescriptionInputMessage, 
	                          Flags: InteractionCallbackDataFlags.Ephemeral
	                         ) 
					    ),
					 ct: ct
					 );

				var option = new RoleMenuOptionModel();
				
				// Parse role
				var roleResult = await GetRoleInputAsync(interaction, ct, option);

				if (roleResult is not Result<RoleMenuOptionModel> rmresult)
					return roleResult;
				
				option = rmresult.Entity;

				var emojiResult = await GetEmojiInputAsync(interaction, ct, option);
				
				var descriptionResult = await GetDescriptionInputAsync(interaction, ct, option);
				
				
				return Result.FromSuccess();

			}
			private async Task<IResult> GetEmojiInputAsync(IInteraction interaction, CancellationToken ct, RoleMenuOptionModel option)
			{
				async Task<IResult> EditResponseAsync(string content)
					=> await _interactions
					   .EditOriginalInteractionResponseAsync
							(
							 interaction.ApplicationID,
							 interaction.Token,
							 content,
							 ct: ct
							);

				while (true)
				{
					var emojiResult = await _interactivity.WaitForMessageAsync(interaction.Member.Value.User.Value, ct);
				}
				
				return default;
			}

			private async Task<IResult> GetDescriptionInputAsync(IInteraction interaction, CancellationToken ct, RoleMenuOptionModel option)
			{
				

				return Result<RoleMenuOptionModel>.FromSuccess(option);
			}
			
			private async Task<IResult> GetRoleInputAsync(IInteraction interaction, CancellationToken ct, RoleMenuOptionModel option)
			{
				async Task<IResult> EditResponseAsync(string content)
					=> await _interactions
					   .EditOriginalInteractionResponseAsync
							(
							 interaction.ApplicationID,
							 interaction.Token,
							 content,
							 ct: ct
							);

				while (true)
				{
					await EditResponseAsync("What role would you like to add? Please mention the role directly! (e.g. @Super Cool Role)");
					
					var roleInput = await _interactivity.WaitForMessageAsync
						(message =>
							!string.IsNullOrEmpty(message.Content)  &&
							message.ChannelID == _context.ChannelID &&
							message.Author.ID == _context.User.ID   &&
							message.MentionedRoles.Any() || message.Content.Equals("cancel", StringComparison.Ordinal)
						);

					if (!roleInput.IsSuccess)
						return roleInput;

					if (roleInput.Entity?.Content.ToLower() is null or "cancel")
					{
						var res = await EditResponseAsync("Cancelled!");

						await Task.Delay(2000, ct);
						return res;
					}

					var roleID = roleInput.Entity.MentionedRoles.First();

					if (_options.Any(r => r.RoleId == roleID.Value))
					{
						var errorResult = await EditResponseAsync("Sorry, but that role is already in use!");
						
						if (errorResult.IsSuccess)
							return errorResult;

						await Task.Delay(2000, ct);
						continue;
					}

					var selfUser   = await _users.GetCurrentUserAsync(ct);
					var selfMember = await _guilds.GetGuildMemberAsync(_context.GuildID.Value, selfUser.Entity.ID, ct);
					var guildRoles = await _guilds.GetGuildRolesAsync(_context.GuildID.Value, ct);

					var selfRoles = selfMember.Entity.Roles.Select(x => guildRoles.Entity.First(y => y.ID == x)).ToArray();
					var role      = guildRoles.Entity.First(x => x.ID == roleID);

					if (role.ID == _context.GuildID.Value)
					{
						var errorResult = await EditResponseAsync("Heh, everyone already has the everyone role!");

						if (!errorResult.IsSuccess)
							return errorResult;

						await Task.Delay(2000, ct);
						continue;
					}

					if (role.Position >= selfRoles.Max(x => x.Position))
					{
						var errorResult = await EditResponseAsync("Sorry, but that role is above my highest role, and I cannot assign it!");

						if (!errorResult.IsSuccess)
							return errorResult;

						await Task.Delay(2000, ct);
						continue;
					}

					option = option with { RoleId = role.ID.Value };

					return Result<RoleMenuOptionModel>.FromSuccess(option);
				}
			}

			private async Task<IResult> EnsureChannelPermissionsAsync(IChannel channel)
			{
				var selfResult = await _users.GetCurrentUserAsync();

				if (!selfResult.IsDefined(out var self))
					return selfResult;
				
				var selfMemberResult = await _guilds.GetGuildMemberAsync(_context.GuildID.Value, self.ID);
				
				if (!selfMemberResult.IsDefined(out var member))
					return selfMemberResult;
				
				var rolesResult = await _guilds.GetGuildRolesAsync(_context.GuildID.Value);

				if (!rolesResult.IsDefined(out var roles))
					return rolesResult;

				var permissions = DiscordPermissionSet.ComputePermissions
					(
					 self.ID,
					 roles.First(r => r.ID == _context.GuildID.Value),
					 roles.Where(r => member.Roles.Contains(r.ID)).ToArray(),
					 channel.PermissionOverwrites.Value
					);

				if (!permissions.HasPermission(DiscordPermission.SendMessages))
					return Result.FromError(new PermissionDeniedError());
				
				return Result.FromSuccess();
			}

			private async Task<IResult> ShowMainMenuAsync(IInteraction interaction, int optionCount)
			{
				var addFullButtonWithState = _addMenuInteractiveButton	with { IsDisabled = optionCount >= 25 };
				var addButtonWithState     = _addMenuSimpleButton		with { IsDisabled = optionCount >= 25 };
				var editButtonWithState    = _addMenuEditButton			with { IsDisabled = optionCount <=  0 };
				var finishButtonWithState  = _addMenuFinishButton		with { IsDisabled = optionCount <=  0 };


				var result = await _interactions.EditOriginalInteractionResponseAsync
					(
					 interaction.ApplicationID,
					 interaction.Token,
					 "Silk! Role Menu Creator V3",
					 components: new IMessageComponent[]
					 {
						 new ActionRowComponent(new IMessageComponent[]
						 {
							 addFullButtonWithState,
							 addButtonWithState,
							 editButtonWithState,
						 }),
						 new ActionRowComponent(new IMessageComponent[]
						 {
							 _addMenuHelpButton,
							 finishButtonWithState,
							 _addMenuCancelButton,
						 })
					 });

				return result;
			}

			private async Task<IResult> InformUserOfChannelErrorAsync()
			{
				var channelResult = await _users.CreateDMAsync(_context.User.ID);

				if (!channelResult.IsDefined(out var DM))
					return channelResult;

				return await _channels.CreateMessageAsync(DM.ID, "Sorry, but I don't have permission to speak in the channel you ran the command in!");
			}
		}
	}
}