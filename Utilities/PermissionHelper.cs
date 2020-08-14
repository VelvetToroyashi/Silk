using DSharpPlus;
using DSharpPlus.Entities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SilkBot.Utilities
{
    public static class PermissionHelper
    {
        public static bool HasPermission(this DiscordRole role, Permissions permission) =>
            role.Permissions.HasPermission(permission);

        public static bool HasPermission(this DiscordMember member, Permissions perm) =>
            member.Roles.Last().Permissions.HasPermission(perm);

        public static IEnumerable<DiscordRole> HasPermission(this DiscordGuild guild, Permissions permission) =>
            guild.Roles.Where(role => role.Value.HasPermission(permission)).Select(t => t.Value);

        public static bool IsModerator(this DiscordMember member) =>
            member.Roles.Any(role => role.Permissions.HasPermission(Permissions.KickMembers));

        public static bool IsAdministrator(this DiscordMember member) =>
            member.Roles.Any(role => role.Permissions.HasPermission(Permissions.Administrator));

        public static bool IsAbove(this DiscordMember target, DiscordMember comparison) =>
            target.Roles.Last().Position > comparison.Roles.Last().Position;

    }
}
