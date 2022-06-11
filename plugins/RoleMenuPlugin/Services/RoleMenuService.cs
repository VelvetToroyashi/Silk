using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;
using Remora.Results;
using RoleMenuPlugin.Database.MediatR;

namespace RoleMenuPlugin;

public class RoleMenuService
{
    public const string RoleMenuButtonPrefix = "rm-menu-initiator";
    
    public const string RoleMenuDropdownPrefix = "rm-menu-selector";
    
    private readonly IMediator                  _mediator;
    private readonly IDiscordRestUserAPI        _users;
    private readonly IDiscordRestGuildAPI       _guilds;
    private readonly IDiscordRestInteractionAPI _interactions;
    private readonly ILogger<RoleMenuService>   _logger;
    
    public RoleMenuService(IMediator mediator, IDiscordRestUserAPI users, IDiscordRestGuildAPI guilds, IDiscordRestInteractionAPI interactions, ILogger<RoleMenuService> logger)
    {
        _mediator     = mediator;
        _users        = users;
        _guilds       = guilds;
        _interactions = interactions;
        _logger       = logger;
    }

    public async Task<Result> HandleButtonAsync(IInteraction interaction)
    {
        await _interactions.CreateInteractionResponseAsync(interaction.ID, interaction.Token,
                                                           new InteractionResponse(InteractionCallbackType.DeferredUpdateMessage));
        
        var roleMenuResult = await _mediator.Send(new GetRoleMenu.Request(interaction.Message.Value.ID.Value));

        if (!roleMenuResult.IsDefined(out var rolemenu))
        {
            var guildID   = interaction.GuildID.Value;
            var channelID = interaction.ChannelID.Value;
            var messageID = interaction.Message.Value.ID;

            await _interactions.CreateFollowupMessageAsync
            (
             interaction.ApplicationID,
             interaction.Token,
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
                 interaction.ApplicationID,
                 interaction.Token,
                 "This role menu is being set up! Please wait until options have been added.",
                 flags: MessageFlags.Ephemeral
                );
                
                return (Result)followupResult;
            }

            var guildRolesResult = await _guilds.GetGuildRolesAsync(interaction.GuildID.Value);
            
            if (!guildRolesResult.IsDefined(out var guildRoles))
                return Result.FromError(guildRolesResult.Error!);

            if (!interaction.Member.IsDefined(out var member))
                throw new InvalidOperationException("Member was not defined in the interaction, but the role menu was found.");

            var dropdown = new SelectMenuComponent
            (
               RoleMenuDropdownPrefix,
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
             interaction.ApplicationID, 
             interaction.Token,
             "Use the dropdown below to assign yourself some roles!",
             flags: MessageFlags.Ephemeral,
             components: new[]
             {
                new ActionRowComponent(new[] { dropdown })
             });
            
           return (Result)result;
        }
        return Result.FromSuccess();
    }

    public async Task<Result> HandleDropdownAsync(IInteraction interaction)
    {
        await _interactions.CreateInteractionResponseAsync(interaction.ID, interaction.Token, new InteractionResponse(InteractionCallbackType.DeferredUpdateMessage));

        if (!interaction.Member.IsDefined(out var member) || !member.User.IsDefined(out var user))
            throw new InvalidOperationException("Member was not defined but the interaction referred to a role menu.");
        
        if (!interaction.GuildID.IsDefined(out var guildID))
            return Result.FromError(new InvalidOperationError("Guild ID was not defined."));
        
        if (!interaction.Message.IsDefined(out var message))
            return Result.FromError(new InvalidOperationError("Message was not defined but the interaction referred to a role menu."));

        if (!interaction.Data.IsDefined(out var data))
            throw new InvalidOperationException("Interaction without data?");

        if (!data.ComponentType.IsDefined(out var type) || type is not ComponentType.SelectMenu)
            return Result.FromError(new InvalidOperationError($"Expected a select menu but got {type}."));
        
        if (!data.Values.IsDefined(out var values))
                values ??= Array.Empty<string>();

        var dropdown = GetDropdownFromMessage(message);
        
        var roleMenuRoleIDs = dropdown
                             .Options
                             .Select
                              (
                               r => Snowflake.TryParse(r.Value, out var ID)
                                  ? ID.Value
                                  : default
                              )
                              .ToArray();

        var roleMenuResult = await _mediator.Send(new GetRoleMenu.Request(interaction.Message.Value.MessageReference.Value.MessageID.Value.Value));

        if (!roleMenuResult.IsDefined(out var roleMenu))
        {
            await _interactions.CreateFollowupMessageAsync(interaction.ApplicationID, interaction.Token, "Sorry, but this role menu is no longer available!", flags: MessageFlags.Ephemeral);
            return Result.FromError(roleMenuResult.Error!);
        }
        
        var parsedRoleIDs = values.Select(ulong.Parse).Select(DiscordSnowflake.New);
        
        var newUserRoles = member.Roles
                                 .Except(roleMenuRoleIDs)
                                 .Union(parsedRoleIDs)
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
        
        if (newUserRoles.Count > member.Roles.Count)
        {
            var assigned = newUserRoles.Except(member.Roles).ToArray();
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
        
        var roleResult = newUserRoles.Count == member.Roles.Count 
            ? Result.FromSuccess()
            : await _guilds.ModifyGuildMemberAsync
              (
               interaction.GuildID.Value,
               user.ID,
               roles: newUserRoles
              );

        if (roleResult.IsSuccess)
        {
            var newOptions = dropdown
                            .Options
                            .Select(r => new SelectOption(r.Label, r.Value, r.Description, r.Emoji, values.Contains(r.Value)))
                            .ToArray();
            
            var interactionResult = await _interactions.EditOriginalInteractionResponseAsync
            (
             interaction.ApplicationID,
             interaction.Token,
             sb.ToString(),
             components: new[]
             {
                 new ActionRowComponent(new [] { new SelectMenuComponent(RoleMenuDropdownPrefix, newOptions, dropdown.Placeholder, 0, dropdown.MaxValues) } )
             }
            );
            
            return (Result)interactionResult;
        }

        return await DisplayRoleMenuErrorAsync(interaction, guildID, roleMenuRoleIDs, roleResult);
    }

    private async Task<Result> DisplayRoleMenuErrorAsync(IInteraction interaction, Snowflake guildID, Snowflake[] roleMenuRoleIDs, Result roleResult)
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

        await _interactions.CreateFollowupMessageAsync(interaction.ApplicationID,
                                                       interaction.Token,
                                                       content.ToString());

        return Result.FromError(roleResult.Error!);
    }


    public ISelectMenuComponent GetDropdownFromMessage(IMessage message)
    {
        var actionRow = message.Components.Value[0] as IActionRowComponent;
        
        var selectMenu = actionRow!.Components[0] as ISelectMenuComponent;
        
        return selectMenu;
    }
}