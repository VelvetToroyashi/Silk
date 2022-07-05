using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Interactivity;
using Remora.Rest.Core;
using Remora.Results;
using RoleMenuPlugin.Database.MediatR;

namespace RoleMenuPlugin;

public class RoleMenuInteractionCommands : InteractionGroup
{
    public const string RoleMenuButtonPrefix = "rm-menu-initiator";
    
    public const string RoleMenuDropdownPrefix = "rm-menu-selector";

    private readonly IMediator                            _mediator;
    private readonly InteractionContext                   _context;
    private readonly IDiscordRestUserAPI                  _users;
    private readonly IDiscordRestGuildAPI                 _guilds;
    private readonly IDiscordRestInteractionAPI           _interactions;
    private readonly ILogger<RoleMenuInteractionCommands> _logger;
    
    public RoleMenuInteractionCommands
    (
        IMediator mediator,
        InteractionContext context,
        IDiscordRestUserAPI users,
        IDiscordRestGuildAPI guilds,
        IDiscordRestInteractionAPI interactions,
        ILogger<RoleMenuInteractionCommands> logger
    )
    {
        _mediator     = mediator;
        _context      = context;
        _users        = users;
        _guilds       = guilds;
        _interactions = interactions;
        _logger       = logger;
    }
    
    [Button(RoleMenuButtonPrefix)]
    // TODO: [RequiresRoleMenu] ?
    public async Task<IResult> HandleButtonAsync()
    {
        var roleMenuResult = await _mediator.Send(new GetRoleMenu.Request(_context.Message.Value.ID.Value));

        if (!roleMenuResult.IsDefined(out var rolemenu))
        {
            var guildID   = _context.GuildID.Value;
            var channelID = _context.ChannelID.Value;
            var messageID = _context.Message.Value.ID;

            await _interactions.CreateFollowupMessageAsync
            (
             _context.ApplicationID,
             _context.Token,
             "Hmm, it looks like this message was a role menu, but it's gone missing.\n"              +
             "Please notify server staff to fix this! Here is a message link for you to give them:\n" +
             $"https://discordapp.com/channels/{guildID}/{channelID}/{messageID}",
             flags: MessageFlags.Ephemeral
            );
            
            _logger.LogError("Role menu defined in {GuildID}/{ChannelID}/{MessageID} but missing from database", guildID, channelID, messageID);
        }
        else
        {
            if (!rolemenu.Options.Any())
            {
                var followupResult = await _interactions.CreateFollowupMessageAsync
                (
                 _context.ApplicationID,
                 _context.Token,
                 "This role menu is being set up! Please wait until options have been added.",
                 flags: MessageFlags.Ephemeral
                );
                
                return (Result)followupResult;
            }

            var guildRolesResult = await _guilds.GetGuildRolesAsync(_context.GuildID.Value);
            
            if (!guildRolesResult.IsDefined(out var guildRoles))
                return Result.FromError(guildRolesResult.Error!);

            if (!_context.Member.IsDefined(out var member))
                throw new InvalidOperationException("Member was not defined in the interaction, but the role menu was found.");

            var dropdown = new SelectMenuComponent
            (
               CustomIDHelpers.CreateSelectMenuID(RoleMenuDropdownPrefix),
               rolemenu
                  .Options
                  .Select(o =>
                   {
                       var roleId   = o.RoleId.ToString();
                       var roleName = guildRoles.FirstOrDefault(r => r.ID.Value == o.RoleId)?.Name ?? "Unknown Role";
                       
                       return new SelectOption(roleName, roleId, o.Description ?? "", GetRoleEmoji(), HasRoleMenuRole());

                       bool HasRoleMenuRole() => member.Roles.Any(r => r.Value == o.RoleId);
                       
                       Optional<IPartialEmoji> GetRoleEmoji()
                       {
                           if (o.EmojiName is null)
                               return default;

                           if (ulong.TryParse(o.EmojiName, out var emojiID))
                               return new PartialEmoji(new Snowflake(emojiID));
                           
                           return new PartialEmoji(default, o.EmojiName);
                       }
                   })
                  .ToArray(),
               "Select the roles you'd like!",
               0,
               rolemenu.MaxSelections is 0 ? rolemenu.Options.Count : rolemenu.MaxSelections
            );

            var result = await _interactions.CreateFollowupMessageAsync
            (
             _context.ApplicationID, 
             _context.Token,
             "Use the dropdown below to assign yourself some roles!",
             flags: MessageFlags.Ephemeral,
             components: new[] { new ActionRowComponent(new[] { dropdown }) }
            );
            
           return (Result)result;
        }
        return Result.FromSuccess();
    }

    [SelectMenu(RoleMenuDropdownPrefix)]
    [RequireContext(ChannelContext.Guild)]
    public async Task<IResult> HandleDropdownAsync(IReadOnlyList<Snowflake> values)
    {
       var dropdown = GetDropdownFromMessage(_context.Message.Value);
        
        var roleMenuRoleIDs = dropdown.Options.Select
        (
         r => Snowflake.TryParse(r.Value, out var ID)
             ? ID.Value
             : default
        )
        .ToArray();

        var roleMenuResult = await _mediator.Send(new GetRoleMenu.Request(_context.Message.Value.MessageReference.Value.MessageID.Value.Value));

        if (!roleMenuResult.IsDefined(out var roleMenu))
        {
            await _interactions.CreateFollowupMessageAsync(_context.ApplicationID, _context.Token, "Sorry, but this role menu is no longer available!", flags: MessageFlags.Ephemeral);
            return Result.FromError(roleMenuResult.Error!);
        }
        
        var newUserRoles = _context.Member.Value.Roles
                                 .Except(roleMenuRoleIDs)
                                 .Union(values)
                                 .ToList();

        var newRoleIDs = newUserRoles.Select(s => s.Value).ToArray();

        var failedExclusions = roleMenu.Options
                                       .Where(o => newRoleIDs.Contains(o.RoleId))
                                       .Where(o => o.MutuallyExclusiveRoleIds.Any())
                                       .Where(o => o.MutuallyExclusiveRoleIds.Intersect(newRoleIDs).Any())
                                       .ToArray();
        
        var failedInclusions = roleMenu.Options
                                       .Where(o => newRoleIDs.Contains(o.RoleId))
                                       .Where(o => o.MutuallyInclusiveRoleIds.Any())
                                       .Where(o => o.MutuallyInclusiveRoleIds.Intersect(newRoleIDs).Count() < o.MutuallyInclusiveRoleIds.Length)
                                       .ToArray();
        
        newUserRoles = newUserRoles.Except(failedExclusions.Union(failedInclusions).Select(u => new Snowflake(u.RoleId))).ToList();
        
        var sb = new StringBuilder();
        
        if (newUserRoles.Count > _context.Member.Value.Roles.Count)
        {
            var assigned = newUserRoles.Except(_context.Member.Value.Roles).ToArray();
            sb.AppendLine($"I've successfully assigned the following roles to you: {string.Join(", ", assigned.Select(r => $"<@&{r.Value}>"))}");
        }

        if (!failedInclusions.Any() && !failedExclusions.Any())
            sb.AppendLine("Enjoy your new roles!");
        else
            sb.AppendLine("There were some errors trying to assign your roles:");

        sb.AppendLine();
        
        if (failedExclusions.Any())
        {
            foreach (var exclusion in failedExclusions)
                sb.AppendLine($"<@&{exclusion.RoleId}> is mutually exclusive with {string.Join(", ", exclusion.MutuallyExclusiveRoleIds.Select(r => $"<@&{r}>"))}");
        }
        
        if (failedInclusions.Any())
        {
            foreach (var inclusion in failedInclusions)
                sb.AppendLine($"<@&{inclusion.RoleId}> is mutually inclusive with {string.Join(", ", inclusion.MutuallyInclusiveRoleIds.Select(r => $"<@&{r}>"))}");
        }
        
        var roleResult = newUserRoles.Count == _context.Member.Value.Roles.Count 
            ? Result.FromSuccess()
            : await _guilds.ModifyGuildMemberAsync
              (
               _context.GuildID.Value,
               _context.User.ID,
               roles: newUserRoles
              );

        if (roleResult.IsSuccess)
        {
            // TODO: Figure out a way to do this without stringifying?
            var newOptions = dropdown
                            .Options
                            .Select(r => new SelectOption(r.Label, r.Value, r.Description, r.Emoji, values.Any(parsed => parsed.Value.ToString() == r.Value)))
                            .ToArray();
            
            var interactionResult = await _interactions.EditOriginalInteractionResponseAsync
            (
             _context.ApplicationID,
             _context.Token,
             sb.ToString(),
             components: new[]
             {
                 new ActionRowComponent(new [] { new SelectMenuComponent(CustomIDHelpers.CreateSelectMenuID(RoleMenuDropdownPrefix), newOptions, dropdown.Placeholder, 0, dropdown.MaxValues) } )
             }
            );
            
            return (Result)interactionResult;
        }

        return await DisplayRoleMenuErrorAsync(_context.GuildID.Value, roleMenuRoleIDs, roleResult);
    }

    private async Task<Result> DisplayRoleMenuErrorAsync(Snowflake guildID, Snowflake[] roleMenuRoleIDs, Result roleResult)
    {
        var selfResult = await _users.GetCurrentUserAsync();

        if (!selfResult.IsSuccess)
            return Result.FromError(selfResult.Error);

        var currentMemberResult = await _guilds.GetGuildMemberAsync(guildID, selfResult.Entity.ID);

        if (!currentMemberResult.IsDefined(out var selfMember))
            return Result.FromError(currentMemberResult.Error!);

        var guildRolesResult = await _guilds.GetGuildRolesAsync(guildID);

        if (!guildRolesResult.IsDefined(out var guildRoles))
            return Result.FromError(guildRolesResult.Error!);

        var highestSelfRole = guildRoles
                             .OrderByDescending(r => r.Position)
                             .Last(r => selfMember.Roles.Contains(r.ID));

        var content = new StringBuilder();

        content.AppendLine("There was an error assigning one or more of the roles you selected.")
               .AppendLine("Please forward this information to a server staff member so they can resolve the issue!");


        var loggedMissingRole = false;
        var loggedHierarchy   = false;


        foreach (var role in roleMenuRoleIDs)
        {
            if (guildRoles.FirstOrDefault(r => r.ID == role) is not { } guildRole)
            {
                content.AppendLine($"Role {role} has since been removed from the server.");

                if (!loggedMissingRole)
                {
                    loggedMissingRole = true;
                    _logger.LogError("One or more roles has gone missing in {GuildID}", guildID);
                }
            }

            else if (guildRole.Position >= highestSelfRole.Position)
            {
                content.AppendLine($"<@&{role}> has been moved above my highest role (<@&{highestSelfRole.ID}>); I cannot (un-)assign it.");

                if (!loggedHierarchy)
                {
                    loggedHierarchy = true;
                    _logger.LogError("One or more roles have become unassignable due to hierarchy in {GuildID}", guildID);
                }
            }
        }

        await _interactions.CreateFollowupMessageAsync
        (
         _context.ApplicationID,
         _context.Token,
         content.ToString()
        );

        return Result.FromError(roleResult.Error!);
    }


    public ISelectMenuComponent GetDropdownFromMessage(IMessage message)
    {
        var actionRow = message.Components.Value[0] as IActionRowComponent;
        
        var selectMenu = actionRow!.Components[0] as ISelectMenuComponent;
        
        return selectMenu!;
    }
}