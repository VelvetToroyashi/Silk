using System.Linq;
using DSharpPlus;
using DSharpPlus.Entities;

namespace Silk.Extensions
{
    public static class PermissionExtensions
    {
        public static bool HasPermission(this DiscordRole role, Permissions permission)
        {
            return role.Permissions.HasPermission(permission);
        }

        public static bool HasPermission(this DiscordMember member, Permissions perm)
        {
            if (member.IsAdministrator() || member.IsOwner)
                return true;

            return !member.Roles.Any()
                ? member.Guild.EveryoneRole.HasPermission(perm)
                : member.Roles.Any(role => role.HasPermission(perm));
        }

        public static bool IsAdministrator(this DiscordMember member)
        {
            return member.Roles.Any(role => role.Permissions.HasPermission(Permissions.Administrator));
        }


        public static string GetRoleMention(this DiscordMember member)
        {
            return member.Roles.Last().Mention;
        }

        public static bool IsAbove(this DiscordMember target, DiscordMember comparison)
        {
            return target.Roles.Any() &&
                   target.Roles.Max(r => r.Position) >=
                   comparison.Roles.Max(r => r.Position);
        }
    }
}