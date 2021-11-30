using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MediatR;
using Microsoft.Extensions.Logging;
using RoleMenuPlugin.Database;
using RoleMenuPlugin.Database.MediatR;

namespace RoleMenuPlugin
{
	public sealed class RoleMenuRoleService
	{
		public const string RoleMenuPrefix = "rmrs-rolemenu"; // Do not rename. //
		private readonly ILogger<RoleMenuRoleService> _logger;

		private readonly IMediator _mediator;

		public RoleMenuRoleService(IMediator mediator, ILogger<RoleMenuRoleService> logger)
		{
			_mediator = mediator;
			_logger = logger;
		}

		public Task Handle(DiscordClient client, ComponentInteractionCreateEventArgs eventArgs)
		{
			_ = HandleInternal(client, eventArgs);
			return Task.CompletedTask;
		}

		private async Task HandleInternal(DiscordClient client, ComponentInteractionCreateEventArgs eventArgs)
		{
			await Task.Yield(); // Yield so we return from the event handler ASAP //

			if (!string.Equals(RoleMenuPrefix, eventArgs.Id) && !ulong.TryParse(eventArgs.Id, out _))
				return;

			await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
			if (eventArgs.Interaction.Data.ComponentType is ComponentType.Button)
			{
				/*
				 * Button handler logic:
				 * TODO: Get roles
				 * TODO: Set role ids to be preselected
				 * TODO: Send ephemeral message
				 */
				RoleMenuModel? menu = await _mediator.Send(new GetRoleMenu.Request(eventArgs.Message.Id));
				List<ulong>? roles = ((DiscordMember)eventArgs.User).Roles.Select(r => r.Id).ToList();

				DiscordSelectComponentOption[]? options = menu.Options
					.Select(o =>
						new DiscordSelectComponentOption($"{eventArgs.Guild.Roles[o.RoleId].Name}", o.RoleId.ToString(CultureInfo.InvariantCulture),
							o.Description, roles.Contains(o.RoleId),
							string.IsNullOrEmpty(o.EmojiName) ? null :
							ulong.TryParse(o.EmojiName, out ulong emoji) ? new(emoji) : new(o.EmojiName)))
					.ToArray();

				var dropdown = new DiscordSelectComponent(eventArgs.Message.Id.ToString(CultureInfo.InvariantCulture), null, options, false, 0, options.Length);

				await eventArgs.Interaction.CreateFollowupMessageAsync(
					new DiscordFollowupMessageBuilder()
						.WithContent("Role picker. Pick one pick a hundred, er... Up to 25, actually.")
						.AddComponents(dropdown)
						.AsEphemeral(true));
			}
			else // Dropdown //
			{
				/*
				 * Dropdown handler logic:
				 * TODO: Get selected values
				 * TODO: Get menu Ids
				 * TODO: Get user roles
				 * TODO: Compare selected to current roles
				 * TODO: Unselected & Present -> Unassign
				 * TODO: Selected & Not Present -> Assign
				 * TODO: Selected & Present | Unselected & Not Present -> Ignore
				 */

				await HandleDropdownAsync(client, eventArgs);
			}
		}

		private async Task HandleDropdownAsync(DiscordClient _, ComponentInteractionCreateEventArgs eventArgs)
		{
			RoleMenuModel? config = await _mediator.Send(new GetRoleMenu.Request(ulong.Parse(eventArgs.Id)));

			var member = (DiscordMember)eventArgs.User;

			IEnumerable<ulong>? selectedMenuIds = eventArgs.Values.Select(ulong.Parse);
			IEnumerable<ulong>? menuRoleIds = config.Options.Select(r => r.RoleId);
			IEnumerable<ulong>? userRoleIds = member.Roles.Select(r => r.Id);


			foreach (ulong id in menuRoleIds)
			{
				// Bot can't assign anymore //
				if (!HasSelfPermissions(eventArgs.Guild))
				{
					await NotifyOfSelfPermissionFailureAsync();
					return;
				}
				// Role was deleted //
				if (!RoleExists(id, eventArgs.Guild, out var role))
				{
					await NotifyOfInvalidRoleAsync(id);
					continue;
				}

				if (FailedHierarchy(eventArgs.Guild, role))
				{
					await NotifyOfHeiarchyFailureAsync(role);
					continue;
				}

				if (selectedMenuIds.Contains(id) && !userRoleIds.Contains(id))
					await member.GrantRoleAsync(role);
				else if (!selectedMenuIds.Contains(id) && userRoleIds.Contains(id))
					await member.RevokeRoleAsync(role);
			}

			async Task NotifyOfInvalidRoleAsync(ulong id)
			{
				await eventArgs.Interaction.CreateFollowupMessageAsync(new()
				{
					IsEphemeral = true,
					Content = "Sorry, but one or more roles has gone missing! Please notify a staff member about this.",
				});

				_logger.LogWarning("A role ({Role}) was not present on guild {Guild}, but is present in a defined role menu.", id, eventArgs.Guild.Id);
			}

			async Task NotifyOfSelfPermissionFailureAsync()
			{
				await eventArgs.Interaction.CreateFollowupMessageAsync(new()
				{
					IsEphemeral = true,
					Content = $"Sorry, but I do not have permission to assign roles! I need the {Permissions.ManageRoles} permission. Please notify a staff member about this.",
				});

				_logger.LogWarning("Requisite permission for role menus on {Guild} is missing! Role-menus are non-functional on this guild.", eventArgs.Guild.Id);
			}

			async Task NotifyOfHeiarchyFailureAsync(DiscordRole role)
			{
				await eventArgs.Interaction.CreateFollowupMessageAsync(new()
				{
					IsEphemeral = true,
					Content = $"Sorry, but I can't assign {role.Mention} because it is above my highest role! Please notify a staff member about this.",
				});

				_logger.LogWarning("A role was defined in a role-menu, but guild hierarchy has changed. Role-menus may no longer work for {Guild}", eventArgs.Guild.Id);
			}
		}

		private bool FailedHierarchy(DiscordGuild eventArgsGuild, DiscordRole role)
		{
			return eventArgsGuild.CurrentMember.Roles.Last().Position <= role.Position;
		}

		private bool HasSelfPermissions(DiscordGuild eventArgsGuild)
		{
			return eventArgsGuild.CurrentMember.Permissions.HasPermission(Permissions.ManageRoles);
		}

		private bool RoleExists(ulong id, DiscordGuild guild, out DiscordRole role)
		{
			return (role = guild.GetRole(id)) is not null;
		}
	}
}