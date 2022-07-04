using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Remora.Commands.Attributes;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Data.MediatR.Guilds;
using Silk.Extensions.Remora;
using Silk.Shared.Constants;

namespace Silk.Commands.Server;

public partial class ConfigCommands
{
    public partial class EditConfigCommands
    {
        [Command("mute")]
        [SuppressMessage("ReSharper", "RedundantBlankLines", Justification = "Readability")]
        [Description("Adjust the configured mute role, or setup native mutes (powered by Discord's Timeout feature).")]
        public async Task<IResult> MuteAsync
        (
            [Description("The role to mute users with.")] 
            IRole? mute = null,
            
            [Option("native")]
            [Description("Whether to use the native mute functionality. This requires the `Timeout Members` permission.")]
            bool? useNativeMute = null
            //It's worth noting that there WAS an option here to have Silk automatically configure the role,
            // but between ratelimits and the fact that permissions suck, it was removed.
        )
        {
            if (mute is null && useNativeMute is null)
            {
                await _channels.CreateReactionAsync(_context.ChannelID, _context.MessageID, $"_:{Emojis.DeclineId}");
                
                return await _channels.CreateMessageAsync(_context.ChannelID, "You must specify either a role or whether to use the native mute functionality.");
            }

            var selfResult = await _guilds.GetCurrentGuildMemberAsync(_users, _context.GuildID.Value);

            if (!selfResult.IsDefined(out var self))
                return selfResult;

            var guildRoles = await _guilds.GetGuildRolesAsync(_context.GuildID.Value);

            if (!guildRoles.IsDefined(out var roles))
                return guildRoles;

            var selfRoles = roles.Where(r => self.Roles.Contains(r.ID)).ToArray();

            var selfPerms = DiscordPermissionSet.ComputePermissions(self.User.Value.ID, roles.First(r => r.ID == _context.GuildID), selfRoles);

            if (useNativeMute is not null && useNativeMute.Value && !selfPerms.HasPermission(DiscordPermission.ModerateMembers))
                return await _channels.CreateMessageAsync(_context.ChannelID, "I don't have permission to timeout members!");

            if (mute is not null)
            {
                if (!selfPerms.HasPermission(DiscordPermission.ManageRoles))
                    return await _channels.CreateMessageAsync(_context.ChannelID, "I don't have permission to assign roles!");

                if (mute.ID == _context.GuildID)
                    return await _channels.CreateMessageAsync(_context.ChannelID, "You can't assign the everyone role as a mute role!");

                if (mute.Position >= selfRoles.Max(r => r.Position))
                    return await _channels.CreateMessageAsync(_context.ChannelID, "This role is above my highest role! I can't assign it.");

                if (mute.Permissions.HasPermission(DiscordPermission.SendMessages))
                    return await _channels.CreateMessageAsync(_context.ChannelID, "This role can send messages. It's not a good idea to assign it to a mute role.");
            }

            await _mediator.Send
                (
                 new UpdateGuildConfig.Request(_context.GuildID.Value)
                 {
                     MuteRoleID    = mute?.ID      ?? default(Optional<Snowflake>),
                     UseNativeMute = useNativeMute ?? default(Optional<bool>)
                 }
                );

            return await _channels.CreateReactionAsync(_context.ChannelID, _context!.MessageID, $"_:{Emojis.ConfirmId}");
        }
    }
}