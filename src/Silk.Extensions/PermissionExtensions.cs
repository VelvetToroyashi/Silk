using System.Linq;
using DSharpPlus;
using DSharpPlus.Entities;

namespace Silk.Extensions
{
    public static class PermissionExtensions
    {
        public static bool HasPermission(this DiscordRole role, Permissions permission) => role.Permissions.HasPermission(permission);

        public static bool HasPermission(this DiscordMember member, Permissions perm)
        {
            return !member.Roles.Any()
                ? member.Guild.EveryoneRole.HasPermission(perm)
                : member.Roles.Any(role => role.HasPermission(perm));
        }

        public static bool IsAdministrator(this DiscordMember member) =>
            member.Roles.Any(role => role.Permissions.HasPermission(Permissions.Administrator));


        public static string GetRoleMention(this DiscordMember member) => member.Roles.Last().Mention;

        public static bool IsAbove(this DiscordMember target, DiscordMember comparison) =>
            target.Roles.Any() &&
            target.Roles.OrderBy(r => r.Position).Last().Position >=
            comparison.Roles.OrderBy(r => r.Position).Last().Position;
    }
}