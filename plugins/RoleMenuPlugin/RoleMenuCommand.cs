using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FuzzySharp;
using MediatR;
using MongoDB.Driver.Core.Bindings;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Results;
using RoleMenuPlugin.Database;
using RoleMenuPlugin.Database.MediatR;

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
			private readonly MessageContext         _context;
			private readonly IDiscordRestUserAPI    _users;
			private readonly IDiscordRestChannelAPI _channels;
			private readonly IDiscordRestGuildAPI   _guilds;

			[Command("create")]
			public async Task<IResult> CreateAsync(IChannel? channel)
			{

				return Result.FromSuccess();
			}
		}
	}
}