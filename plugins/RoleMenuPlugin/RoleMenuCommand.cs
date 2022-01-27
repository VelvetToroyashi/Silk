using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
using Remora.Rest.Core;
using Remora.Results;
using RoleMenuPlugin.Database;

namespace RoleMenuPlugin
{
	/// <summary>
	/// The command module responsible for creating, modifying, and deleting role menus.
	/// </summary>
	[Group("rolemenu")]
	[Description("Role menu related commands.")]
	public sealed class RoleMenuCommand : CommandGroup
	{
		private static readonly Dictionary<Snowflake, (Snowflake, List<Snowflake>)> _sessions = new();
		
		public class CreateCommand : CommandGroup
		{
			private readonly MessageContext            _context;
			private readonly IDiscordRestUserAPI       _users;
			private readonly IDiscordRestChannelAPI    _channels;
			private readonly IDiscordRestGuildAPI      _guilds;
			private readonly ILogger<RoleMenuCommand>  _logger;
			
			public CreateCommand
			(
				MessageContext           context,
				IDiscordRestUserAPI      users,
				IDiscordRestChannelAPI   channels,
				IDiscordRestGuildAPI     guilds,
				// ReSharper disable once ContextualLoggerProblem
				ILogger<RoleMenuCommand> logger
			)
			{
				_context      = context;
				_users        = users;
				_channels     = channels;
				_guilds       = guilds;
				_logger       = logger;
			}

			[Command("create")]
			[RequireDiscordPermission(DiscordPermission.ManageChannels)]
			public async Task<IResult> CreateAsync
			(
				[Description("The channel the role menu will be created in.\n" +
				             "This channel must be a text channel, and must allow sending messages.")]
				IChannel? channel = null,
				params IRole[]? roles
			)
			{
				var sessionExists = _sessions.ContainsKey(_context.User.ID);

				if (sessionExists)
				{
					await _channels.CreateMessageAsync(_context.ChannelID, "You already have an active role menu session.");
					return Result.FromSuccess();
				}
				
				if (channel is null)
				{
					var currentChannelResult = await _channels.GetChannelAsync(_context.ChannelID);

					if (currentChannelResult.IsSuccess)
					{
						channel = currentChannelResult.Entity;
					}
					else
					{
						_logger.LogError("User appears to be in an invalid channel: {UserID}, {ChannelID}", _context.User.ID, _context.ChannelID);
						return currentChannelResult;
					}
				}

				var permsResult = await EnsureChannelPermissionsAsync(channel);

				if (permsResult is not Result<(IReadOnlyList<IRole>, IRole)> permissionResultWithValue)
					return permsResult;
				
				var (selfRoles, role) = permissionResultWithValue.Entity;

				_sessions[_context.User.ID] = (channel.ID, (roles ?? Array.Empty<IRole>())
				                                          .Where(r => r.Position <= selfRoles.Max(r => r.Position))
				                                          .Select(r => r.ID)
				                                          .ToList());
				
				
				return await _channels.CreateReactionAsync(_context.ChannelID, _context.MessageID, "✅");
			}
			
			private async Task<IResult> EnsureChannelPermissionsAsync(IChannel channel)
			{
				var rolesResult = await _guilds.GetGuildRolesAsync(_context.GuildID.Value);

				if (!rolesResult.IsDefined(out var guildRoles))
					return rolesResult;

				var selfResult = await _users.GetCurrentUserAsync();

				if (!selfResult.IsDefined(out var self))
					return selfResult;

				var memberResult = await _guilds.GetGuildMemberAsync(_context.GuildID.Value, self.ID);

				if (!memberResult.IsDefined(out var member))
					return memberResult;

				var everyoneRole = guildRoles.First(r => r.ID == _context.GuildID.Value);
				var selfRoles    = guildRoles.Where(r => member.Roles.Contains(r.ID)).ToArray();
				
				var channelPermissionResult = DiscordPermissionSet
				   .ComputePermissions(_context.User.ID,
				                       everyoneRole,
				                       selfRoles,
				                       channel.PermissionOverwrites.Value);

				if (!channelPermissionResult.HasPermission(DiscordPermission.SendMessages))
				{
					await _channels.CreateMessageAsync(_context.ChannelID, "I can't send messages to that channel!");
					return Result.FromError(new PermissionDeniedError());
				}
				
				return Result<(IReadOnlyList<IRole>, IRole)>.FromSuccess((selfRoles, everyoneRole));
			}
		}
	}
}