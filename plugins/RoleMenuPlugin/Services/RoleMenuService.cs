using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
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
    
    private readonly IMediator                  _mediator;
    private readonly IDiscordRestUserAPI        _users;
    private readonly IDiscordRestGuildAPI       _guilds;
    private readonly IDiscordRestInteractionAPI _interactions;
    public RoleMenuService(IMediator mediator, IDiscordRestUserAPI users, IDiscordRestGuildAPI guilds, IDiscordRestInteractionAPI interactions)
    {
        _mediator     = mediator;
        _users        = users;
        _guilds       = guilds;
        _interactions = interactions;
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
        }
        else
        {
            var guildRolesResult = await _guilds.GetGuildRolesAsync(interaction.GuildID.Value);
            
            if (!guildRolesResult.IsDefined(out var guildRoles))
                return Result.FromError(guildRolesResult.Error!);

            if (!interaction.Member.IsDefined(out var member))
                throw new InvalidOperationException("Member was not defined in the interaction, but the role menu was found.");

            var dropdown = new SelectMenuComponent
                (
                   "silk-rolemenu",
                   rolemenu
                      .Options
                      .Select(o =>
                       {
                           var roleId   = o.RoleId.ToString();
                           var roleName = guildRoles.FirstOrDefault(r => r.ID.Value == o.RoleId)?.Name ?? "Unknown Role";
                           
                           return new SelectOption
                               (
                                Label: roleName,
                                Value: roleId,
                                Description: default,
                                Emoji: GetRoleEmoji(),
                                IsDefault: HasRoleMenuRole()
                               );

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
                   rolemenu.Options.Count
                );

            var result = await _interactions
               .CreateFollowupMessageAsync
                    (
                     interaction.ApplicationID, 
                     interaction.Token,
                     "Use the dropdown below to assign yourself some roles!",
                     components: new[]
                     {
                        new ActionRowComponent(new[] { dropdown })
                     });
            return result.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(result.Error);
        }
        return Result.FromSuccess();
    }

    public async Task<Result> HandleDropdownAsync(IInteraction interaction)
    {
        await _interactions.CreateInteractionResponseAsync(interaction.ID, interaction.Token,
                                                           new InteractionResponse(InteractionCallbackType.DeferredUpdateMessage));

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

        var roleMenuResult = await _mediator.Send(new GetRoleMenu.Request(message.ID.Value));

        if (!roleMenuResult.IsDefined(out var roleMenu))
        {
            await _interactions.CreateFollowupMessageAsync(interaction.ApplicationID,
                                                           interaction.Token,
                                                           "The role menu you selected was not found! This is likely a bug in Silk!, and should be reported.\n" +
                                                           "You can join the support server [here](https://discord.gg/XsHcuvUWda) to report this.\n\n"          +
                                                           "In your report, feel free to include the information below:\n"                                      +
                                                           "Role deleted between invocation and action.\n"                                                      +
                                                           $"Guild ID: {interaction.GuildID.Value}\n"                                                           +
                                                           $"Channel ID: {interaction.ChannelID.Value}\n"                                                       +
                                                           $"Message ID: {interaction.Message.Value.ID.Value}\n");

            //TODO: Log
            return Result.FromError(new NotFoundError("Role menu was defined but could not be found at the time of actioning."));
        }
        else
        {
            if (!data.Values.IsDefined(out var values))
                values ??= Array.Empty<string>();

            var roleMenuRoleIDs = roleMenu.Options.Select(r => new Snowflake(r.RoleId));
            var parsedRoleIDs   = values.Select(ulong.Parse).Select(ID => new Snowflake(ID));

            var newUserRoles = member.Roles
                                     .Except(roleMenuRoleIDs)
                                     .Union(parsedRoleIDs)
                                     .ToArray();

            var roleResult = await _guilds.ModifyGuildMemberAsync(
                                                                  interaction.GuildID.Value,
                                                                  user.ID,
                                                                  roles: newUserRoles
                                                                 );

            if (roleResult.IsSuccess)
            {
                await _interactions.EditOriginalInteractionResponseAsync(
                                                                         interaction.ApplicationID,
                                                                         interaction.Token,
                                                                         "Done! Enjoy your new roles!"
                                                                        );
                return Result.FromSuccess();
            }

            var selfResult = await _users.GetCurrentUserAsync();

            if (!selfResult.IsSuccess)
                return Result.FromError(selfResult.Error);

            var currentMemberResult = await _guilds.GetGuildMemberAsync(guildID, selfResult.Entity.ID);

            if (!currentMemberResult.IsDefined(out var selfMember))
                return Result.FromError(currentMemberResult.Error);

            var guildRolesResult = await _guilds.GetGuildRolesAsync(guildID);

            if (!guildRolesResult.IsDefined(out var guildRoles))
                return Result.FromError(guildRolesResult.Error!);

            var highestSelfRole = guildRoles
                                 .OrderByDescending(r => r.Position)
                                 .Last(r => selfMember.Roles.Contains(r.ID));

            var content = new StringBuilder();

            content.AppendLine("There was an error assigning one or more of the roles you selected.")
                   .AppendLine("Please forward this information to a server staff member so they can resolve the issue!");

            foreach (var role in roleMenuRoleIDs)
            {
                if (guildRoles.FirstOrDefault(r => r.ID == role) is not { } guildRole)
                    content.AppendLine($"Role {role} has since been removed from the server.");

                else if (guildRole.Position >= highestSelfRole.Position)
                    content.AppendLine($"<@&{role}> has been moved above my highest role (<@&{highestSelfRole.ID}>); I cannot (un-)assign it.");
            }

            await _interactions.CreateFollowupMessageAsync(interaction.ApplicationID,
                                                           interaction.Token,
                                                           content.ToString());
            
            return Result.FromError(roleResult.Error!);
        }
    }
}