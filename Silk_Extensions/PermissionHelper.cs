using System.Collections.Generic;
using System.Linq;
using DSharpPlus;
using DSharpPlus.Entities;

namespace SilkBot.Extensions
{
    public static class PermissionHelper
    {
        public static bool HasPermission(this DiscordRole role, Permissions permission)
        {
            return role.Permissions.HasPermission(permission);
        }

        public static bool HasPermission(this DiscordMember member, Permissions perm)
        {
            return !member.HasRoles()
                ? member.Guild.EveryoneRole.HasPermission(perm)
                : member.Roles.Any(role => role.HasPermission(perm));
        }


        public static IEnumerable<DiscordRole> HasPermission(this DiscordGuild guild, Permissions permission)
        {
            return guild.Roles.Where(role => role.Value.HasPermission(permission)).Select(t => t.Value);
        }

        public static bool IsModerator(this DiscordMember member)
        {
            return member.Roles.Any(role => role.Permissions.HasPermission(Permissions.KickMembers));
        }

        public static bool IsAdministrator(this DiscordMember member)
        {
            return member.Roles.Any(role => role.Permissions.HasPermission(Permissions.Administrator));
        }

        public static bool HasRoles(this DiscordMember member)
        {
            return member.Roles.Any();
        }

        public static string GetHighestRoleMention(this DiscordMember member)
        {
            return member.Roles.Last().Mention;
        }

        public static bool IsAbove(this DiscordMember target, DiscordMember comparison)
        {
            if (!target.Roles.Any())
                return false;
            else
                return target.Roles.Last().Position > comparison.Roles.Last().Position;
        }
    }
}