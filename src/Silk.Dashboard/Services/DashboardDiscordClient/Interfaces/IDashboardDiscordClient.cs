#nullable enable

using Remora.Discord.API.Abstractions.Objects;
using Remora.Rest.Core;

namespace Silk.Dashboard.Services.DashboardDiscordClient.Interfaces;

public interface IDashboardDiscordClient
{
    Task<IUser?>                        GetCurrentUserAsync(bool forceRefresh = false);
    Task<IReadOnlyList<IPartialGuild>?> GetCurrentUserGuildsAsync(bool   forceRefresh = false);

    Task<IReadOnlyList<IPartialGuild>?> GetCurrentUserGuildsByPermissionsAsync
    (
        DiscordPermission permissions,
        bool              forceRefresh = false
    );

    IReadOnlyList<IPartialGuild>? FilterGuildsByPermission
    (
        IReadOnlyList<IPartialGuild>? guilds,
        DiscordPermission             permission
    );

    Task<IPartialGuild?> GetCurrentUserGuildByIdAndPermissionAsync
    (
        Snowflake         guildId,
        DiscordPermission permission,
        bool              forceRefresh = false
    );
}