using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FuzzySharp;
using MediatR;
using Microsoft.Extensions.Logging;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Results;
using Remora.Rest.Core;
using Remora.Results;
using RoleMenuPlugin.Database;
using RoleMenuPlugin.Database.MediatR;
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
				MessageContext context,
				IDiscordRestUserAPI users,
				IDiscordRestChannelAPI channels,
				IDiscordRestGuildAPI guilds,
				InteractivityExtension interactivity,
				ILogger<RoleMenuCommand> logger,
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
				_logger.LogTrace("Enter loop.");
				while (true)
				{
					_logger.LogTrace("Looped.");
					
					var selectionResult = await _interactivity.WaitForButtonAsync(_context.User, message, this.CancellationToken);
					
					_logger.LogTrace("Got input: Success: {IsSuccess}, Defined: {IsDefined}", selectionResult.IsSuccess, selectionResult.IsDefined());
					
					
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
						
						_ => Result.FromSuccess() // An exception should be thrown here, as it's outside what should be possible.
					};
					
					await ShowMainMenuAsync(selection, _options.Count);
				}

				return Result.FromSuccess();
			}

			private async Task<IResult> CreateInteractiveAsync(IInteraction interaction, CancellationToken ct)
			{
				await _interactions.CreateInteractionResponseAsync(interaction.ID, interaction.Token,
				                                                   new InteractionResponse(InteractionCallbackType.UpdateMessage,
				                                                                           new InteractionCallbackData(Content: "Poggers.")),
				                                                   ct: ct);

				await Task.Delay(2000);
				return Result.FromSuccess();
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