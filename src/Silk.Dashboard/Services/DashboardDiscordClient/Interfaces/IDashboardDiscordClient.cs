#nullable enable

using Remora.Discord.API.Abstractions.Objects;
using Remora.Rest.Core;

namespace Silk.Dashboard.Services.DashboardDiscordClient.Interfaces;

public interface IDashboardDiscordClient
{
    Task<IUser?>                        GetCurrentUserAsync(bool forceRefresh = false);
    Task<IReadOnlyList<IPartialGuild>?> GetAllGuildsAsync(bool   forceRefresh = false);

    Task<IReadOnlyList<IPartialGuild>?> GetGuildsByPermissionAsync
    (
        DiscordPermission permissions,
        bool              forceRefresh = false
    );

    IReadOnlyList<IPartialGuild>? FilterGuildsByPermission
    (
        IReadOnlyList<IPartialGuild>? guilds,
        DiscordPermission             permission
    );

    Task<IPartialGuild?> GetGuildByIdAndPermissionAsync
    (
        Snowflake         guildId,
        DiscordPermission permission,
        bool              forceRefresh = false
    );
}