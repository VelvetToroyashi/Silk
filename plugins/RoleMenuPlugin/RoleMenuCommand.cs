using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
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

// ReSharper disable once ContextualLoggerProblem
// ReSharper disable RedundantBlankLines
namespace RoleMenuPlugin;

/// <summary>
/// The command module responsible for creating, modifying, and deleting role menus.
/// </summary>
[Group("rolemenu")]
[Description("Role Menu related commands.\n"                                                                                +
             "V3 has simplified and streamlined this process, using a command-based creation and modification system. \n\n" +
             "**Quick-start guilde**:\n"                                                                                    +
             "`rolemenu create` - Creates a new menu, optionally adding roles to it.\n"                                     +
             "`rolemenu add` - Adds roles to a role menu")]
public sealed class RoleMenuCommand : CommandGroup
{
    private readonly MessageContext           _context;
    private readonly IDiscordRestUserAPI      _users;
    private readonly IDiscordRestChannelAPI   _channels;
    private readonly IDiscordRestGuildAPI     _guilds;
    private readonly ILogger<RoleMenuCommand> _logger;
    private readonly IMediator                _mediator;
    
    public RoleMenuCommand
    (
        MessageContext           context,
        IDiscordRestUserAPI      users,
        IDiscordRestChannelAPI   channels,
        IDiscordRestGuildAPI     guilds,
        ILogger<RoleMenuCommand> logger,
        IMediator mediator
    )
    {
        _context  = context;
        _users    = users;
        _channels = channels;
        _guilds   = guilds;
        _logger   = logger;
        _mediator = mediator;
    }

    [Command("create")]
    [RequireDiscordPermission(DiscordPermission.ManageChannels)]
    [Description("Initiates a role menu creation session. Roles can be added at this stage via mentions (@Role) or IDs (123456789).")]
    public async Task<IResult> CreateAsync
    (
        [Option('c', "channel")]
        [Description("The channel the role menu will be created in.\n" +
                     "This channel must be a text channel, and must allow sending messages.")]
        IChannel? channel = null,
            
        [Description("The roles to add to the role menu; this is optional, but any roles above my own.\n" +
                     "Any roles above my own and the @everyone role will be discarded!")]
        IRole[]? roles = null
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
                _logger.LogError("User appears to be in an invalid channel: {UserID}, {ChannelID}", _context.User.ID, _context.ChannelID);
                return currentChannelResult;
            }
        }

        var permsResult = await EnsureChannelPermissionsAsync(channel);

        if (permsResult is not Result<(IReadOnlyList<IRole>, IRole)> permissionResultWithValue)
            return permsResult;

        var (selfRoles, everyoneRole) = permissionResultWithValue.Entity;
        
        roles = (roles ?? Array.Empty<IRole>())
                   .DistinctBy(r => r.ID)
                   .Where(r => r.Position <= selfRoles.Max(sr => sr.Position))
                   .Except(new[] { everyoneRole })
                   .ToArray();

        var roleMenuResult = await FinalizeRoleMenuAsync(roles);

        if (!roleMenuResult.IsSuccess)
        {
            await _channels.CreateReactionAsync(_context.ChannelID, _context.MessageID, "❌");
            
            await _channels.CreateMessageAsync(channel.ID, "Failed to create role menu: " + roleMenuResult.Error!.Message);
            
            return roleMenuResult;
        }
        
        await _channels.CreateMessageAsync(_context.ChannelID, $"RoleMenu ID: {((Result<IMessage>)roleMenuResult).Entity.ID}.\n" +
                                                               $"Use this ID to edit the menu!");
        
        
        return await _channels.CreateReactionAsync(_context.ChannelID, _context.MessageID, "✅");
    }

    private async Task<IResult> FinalizeRoleMenuAsync(IReadOnlyList<IRole> roles)
    {
        var roleMenuMessageResult = await _channels
           .CreateMessageAsync
                (
                   _context.ChannelID,
                   !roles.Any()
                       ? "This role menu is being set up, please wait!"
                       : "**Role Menu!**\n"                               +
                         "Use the button below to select your roles!\n\n" +
                         "Available roles:\n"                             +
                         string.Join('\n', roles.Select(r => $"<@&{r.ID}>")),
                   components:
                   new[]
                   {
                       new ActionRowComponent(new[]
                       {
                           new ButtonComponent(ButtonComponentStyle.Success, "Get Roles!", CustomID: "rm-button")
                       })
                   }
                );

        var roleMenu = await _mediator.Send
            (
             new CreateRoleMenu.Request(new()
             {
                 ChannelId = _context.ChannelID.Value,
                 MessageId = roleMenuMessageResult.Entity.ID.Value,
                 Options   = roles.Select(r => new RoleMenuOptionModel() { RoleId = r.ID.Value, RoleName = r.Name }).ToList()
             })
            );
        
        if (!roleMenu.IsSuccess)
            return roleMenu;

        return roleMenuMessageResult;
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