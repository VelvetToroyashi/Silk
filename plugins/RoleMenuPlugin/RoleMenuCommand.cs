using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
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
[PublicAPI]
[Group("rolemenu")]
[RequireContext(ChannelContext.Guild)]
[RequireDiscordPermission(DiscordPermission.ManageChannels)]
[Description("Role Menu related commands.\n" +
             "V3 has simplified and streamlined this process, using a command-based creation and modification system. \n\n" +
             "**Quick-start guilde**:\n" +
             "`rolemenu create` - Creates a new menu, optionally adding roles to it.\n" +
             "`rolemenu add` - Adds roles to a role menu")]
public sealed class RoleMenuCommand : CommandGroup
{
    private readonly IMediator                _mediator;
    private readonly MessageContext           _context;
    private readonly IDiscordRestUserAPI      _users;
    private readonly IDiscordRestGuildAPI     _guilds;
    private readonly IDiscordRestChannelAPI   _channels;
    private readonly ILogger<RoleMenuCommand> _logger;
    
    public RoleMenuCommand
    (
        IMediator                mediator,
        MessageContext           context,
        IDiscordRestUserAPI      users,
        IDiscordRestGuildAPI     guilds,
        IDiscordRestChannelAPI   channels,
        ILogger<RoleMenuCommand> logger
    )
    {
        _context  = context;
        _users    = users;
        _channels = channels;
        _guilds   = guilds;
        _logger   = logger;
        _mediator = mediator;
    }

    [Command("create", "c")]
    [Description("Initiates a role menu creation session. Roles can be added at this stage via mentions (@Role) or IDs (123456789).\n" +
                 "It's recommended to create a role menu in a channel users cannot view whilst setting up the role menu.\n" +
                 "The role menu message is sent immediately and updated as you update your role menu, which may confuse vigilant users!")]
    public async Task<IResult> CreateAsync
    (
        [Description("The channel the role menu will be created in.\n" +
                     "This channel must be a text channel, and must allow sending messages.")]
        IChannel channel,
        [Description("The roles to add to the role menu; this is optional, but any roles above my own.\n" +
                     "Any roles above my own and the @everyone role will be discarded!")]
        IRole[]? roles = null
    )
    {
        var permsResult = await EnsureChannelPermissionsAsync(channel);

        if (permsResult is not Result<(IReadOnlyList<IRole>, IRole)> permissionResultWithValue)
            return permsResult;

        var (selfRoles, everyoneRole) = permissionResultWithValue.Entity;

        roles = (roles ?? Array.Empty<IRole>())
               .DistinctBy(r => r.ID)
               .Where(r => r.Position <= selfRoles.Max(sr => sr.Position))
               .Except(new[] {everyoneRole})
               .ToArray();

        var roleMenuResult = await CreateRoleMenuMessageAsync(channel.ID, roles);

        if (!roleMenuResult.IsSuccess)
        {
            await _channels.CreateReactionAsync(_context.ChannelID, _context.MessageID, "❌");

            await _channels.CreateMessageAsync(channel.ID,
                                               "Failed to create role menu: " + roleMenuResult.Error!.Message);

            return roleMenuResult;
        }

        await _channels.CreateMessageAsync(_context.ChannelID,
                                           $"RoleMenu ID: {((Result<IMessage>) roleMenuResult).Entity.ID}.\n" +
                                           $"Use this ID to edit the menu!");


        return await _channels.CreateReactionAsync(_context.ChannelID, _context.MessageID, "✅");
    }

    [Command("add", "a")]
    [Description("Adds one or more role(s) to the role menu.\n" +
                 "Roles can be added at this stage via mentions (@Role) or IDs (123456789).\n")]
    public Task<IResult> AddAsync
        (
            [Description("A link to the role menu message. This can be obtained by right clicking or holding the message and selecting `Copy Message Link`.")]
            IMessage message,
            
            [Description("The roles to add to the role menu. Roles above my own and the @everyone role will be discarded!")]
            IRole[] roles
        )
        => AddAsync(message.ID, roles);
    
    [Command("add", "a")]
    [Description("Adds one or more role(s) to the role menu.\n"                                             +
                 "Roles can be added at this stage via mentions (@Role) or IDs (123456789)")]
    public async Task<IResult> AddAsync
    (
        [Description("The ID of the role menu message, provided when creating the role menu.")]
        Snowflake messageID,
        
        [Description("The roles to add to the role menu. Roles above my own and the @everyone role will be discarded!")]
        IRole[] roles
    )
    {
        if (!roles.Any())
        {
            await _channels.CreateReactionAsync(_context.ChannelID, _context.MessageID, "❌");
            return await DeleteAfter("You must provide at least one role!", TimeSpan.FromSeconds(5));
        }

        var roleMenuResult = await _mediator.Send(new GetRoleMenu.Request(messageID.Value));

        if (!roleMenuResult.IsSuccess)
        {
            await _channels.CreateReactionAsync(_context.ChannelID, _context.MessageID, "❌");
            return await DeleteAfter("I don't see a role menu with that ID, are you sure it still exists?", TimeSpan.FromSeconds(5));
        }

        var roleResult = await GetRolesAsync();

        if (!roleResult.IsSuccess)
            return roleResult;

        var (everyoneRole, allRoles) = roleResult.Entity;

        var roleMenu = roleMenuResult.Entity;

        var duplicateRoles = roles.Where(r => roleMenu.Options.Select(rm => rm.RoleId).Contains(r.ID.Value));
        
        var rolesToAdd = roles
                        .DistinctBy(r => r.ID)
                        .Where(r => r.Position <= allRoles.Max(sr => sr.Position))
                        .Except(new[] {everyoneRole})
                        .Except(duplicateRoles)
                        .ToArray();

        if (!rolesToAdd.Any())
            return await _channels.CreateReactionAsync(_context.ChannelID, _context.MessageID, "✅");

        if (rolesToAdd.Length + roleMenu.Options.Count > 25)
        {
            await _channels.CreateReactionAsync(_context.ChannelID, _context.MessageID, "❌");

            return await
                DeleteAfter($"You can only have 25 roles in a role menu! You're {roles.Length + roleMenu.Options.Count - 25} roles over!",
                            TimeSpan.FromSeconds(15));
        }

        roleMenu.Options.AddRange(rolesToAdd.Select(r => new RoleMenuOptionModel()
                                                        {RoleId = r.ID.Value, RoleName = r.Name}));
        
        await _mediator.Send(new UpdateRoleMenu.Request(messageID, roleMenu.Options));

        var roleMenuMessageResult = await _channels.EditMessageAsync
            (
             new(roleMenu.ChannelId),
             messageID,
             "**Role Menu!**\n"                               +
             "Use the button below to select your roles!\n\n" +
             "Available roles:\n"                             +
             string.Join('\n', roleMenu.Options.Select(r => $"<@&{r.RoleId}>"))
            );

        if (!roleMenuMessageResult.IsSuccess)
        {
            _logger.LogWarning($"Failed to edit role menu message {roleMenu.ChannelId}/{roleMenu.MessageId}.");
            await _channels.CreateReactionAsync(_context.ChannelID, _context.MessageID, "❌");
            
            return await DeleteAfter("I couldn't edit the role menu message, are you sure it still exists?", TimeSpan.FromSeconds(10));
        }

        return await _channels.CreateReactionAsync(_context.ChannelID, _context.MessageID, "✅");
    }



    
    [Command("remove", "rm", "r")]
    [Description("Removes one or more roles from the role menu.")]
    public Task<IResult> RemoveAsync
    (
        [Description("A link to the role menu message. This can be obtained by right clicking or holding the message and selecting `Copy Message Link`.")]
        IMessage message,
        
        [Description("The roles to remove, roles can be removed by mention (@Role) or by ID (123456789).\n" +
                     "You must have at least one role in the role menu, however.")]
        IRole[] roles
    )
    => RemoveAsync(message.ID, roles);

    [Command("remove", "rm", "r")]
    [Description("Removes one or more roles from the role menu.")]
    public async Task<IResult> RemoveAsync
    (
        [Description("The ID of the role menu message.")]
        Snowflake messageID,

        [Description("The roles to remove, roles can be removed by mention (@Role) or by ID (123456789).\n" +
                     "You must have at least one role in the role menu, however.")]
        IRole[] roles
    )
    {

        if (!roles.Any())
        {
            await _channels.CreateReactionAsync(_context.ChannelID, _context.MessageID, "❌");
            return await DeleteAfter("You must provide at least one role!", TimeSpan.FromSeconds(5));
        }
        
        
        var roleMenuResult = await _mediator.Send(new GetRoleMenu.Request(messageID.Value));

        if (!roleMenuResult.IsSuccess)
        {
            await _channels.CreateReactionAsync(_context.ChannelID, _context.MessageID, "❌");
            return await DeleteAfter("I don't see a role menu with that ID, are you sure it still exists?", TimeSpan.FromSeconds(5));
        }
        
        var roleMenu = roleMenuResult.Entity;

        var roleOptions = roleMenu.Options;

        var newRoles = roleOptions.ExceptBy(roles.Select(r => r.ID.Value), o => o.RoleId).ToArray();
        
        if (!newRoles.Any())
        {
            await _channels.CreateReactionAsync(_context.ChannelID, _context.MessageID, "❌");
            
            return await DeleteAfter("It appears you're trying to clear the role menu." +
                                     "\n\nPerhaps you meant to use `rolemenu delete` instead?", TimeSpan.FromSeconds(25));
        }
        
        await _mediator.Send(new UpdateRoleMenu.Request(messageID, newRoles));
        
        var roleMenuMessageResult = await _channels.EditMessageAsync
            (
             new(roleMenu.ChannelId),
             messageID,
             "**Role Menu!**\n"                               +
             "Use the button below to select your roles!\n\n" +
             "Available roles:\n"                             +
             string.Join('\n', newRoles.Select(r => $"<@&{r.RoleId}>"))
            );

        if (!roleMenuMessageResult.IsSuccess)
        {
            _logger.LogWarning($"Failed to edit role menu message {roleMenu.ChannelId}/{roleMenu.MessageId}.");
            await _channels.CreateReactionAsync(_context.ChannelID, _context.MessageID, "❌");
            
            return await DeleteAfter("I couldn't edit the role menu message, are you sure it still exists?", TimeSpan.FromSeconds(10));
        }
        
        return await _channels.CreateReactionAsync(_context.ChannelID, _context.MessageID, "✅");
    }

    
    private async Task<IResult> DeleteAfter(string content, TimeSpan delay)
    {
        var sendResult = await _channels.CreateMessageAsync(_context.ChannelID, content);

        if (!sendResult.IsSuccess)
            return sendResult;

        await Task.Delay(delay);

        return await _channels.DeleteMessageAsync(_context.ChannelID, sendResult.Entity.ID);
    }
    
    private async Task<IResult> CreateRoleMenuMessageAsync(Snowflake channelID, IReadOnlyList<IRole> roles)
    {
        var roleMenuMessageResult = await _channels
           .CreateMessageAsync
                (
                 channelID,
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
                         new ButtonComponent(ButtonComponentStyle.Success, "Get Roles!", CustomID: RoleMenuService.RoleMenuButtonPrefix)
                     })
                 }
                );

        if (!roleMenuMessageResult.IsSuccess)
            return roleMenuMessageResult;

        var roleMenu = await _mediator.Send
            (
             new CreateRoleMenu.Request(new()
             {
                 ChannelId = channelID.Value,
                 MessageId = roleMenuMessageResult.Entity.ID.Value,
                 Options = roles.Select(r => new RoleMenuOptionModel() {RoleId = r.ID.Value, RoleName = r.Name})
                                .ToList()
             })
            );

        if (!roleMenu.IsSuccess)
            return roleMenu;

        return roleMenuMessageResult;
    }

    public Task<IResult> DeleteAsync
        (
            IMessage message,

            IRole[] roles
        ) => Task.FromResult((IResult)Result.FromSuccess());

    private async Task<IResult> EnsureChannelPermissionsAsync(IChannel channel)
    {
        var roleResult = await GetRolesAsync();

        if (!roleResult.IsSuccess)
            return roleResult;

        var (everyoneRole, selfRoles) = roleResult.Entity;

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

    private async Task<Result<(IRole everyoneRole, IRole[] selfRole)>> GetRolesAsync()
    {
        var rolesResult = await _guilds.GetGuildRolesAsync(_context.GuildID.Value);

        if (!rolesResult.IsDefined(out var guildRoles))
            return Result<(IRole, IRole[])>.FromError(rolesResult.Error!);

        var selfResult = await _users.GetCurrentUserAsync();

        if (!selfResult.IsDefined(out var self))
            return Result<(IRole, IRole[])>.FromError(selfResult.Error!);

        var memberResult = await _guilds.GetGuildMemberAsync(_context.GuildID.Value, self.ID);

        if (!memberResult.IsDefined(out var member))
            return Result<(IRole, IRole[])>.FromError(memberResult.Error!);

        var everyoneRole = guildRoles.First(r => r.ID == _context.GuildID.Value);
        var selfRoles    = guildRoles.Where(r => member.Roles.Contains(r.ID)).ToArray();

        return Result<(IRole everyoneRole, IRole[] selfRole)>.FromSuccess((everyoneRole, selfRoles));
    }
}