﻿#nullable enable

using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Rest.Core;
using Silk.Dashboard.Services.DashboardDiscordClient.Interfaces;

namespace Silk.Dashboard.Services.DashboardDiscordClient;

public class DashboardDiscordClient : IDashboardDiscordClient
{
    private readonly IDiscordRestUserAPI  _userApi;

    private IUser?                        _cachedUser;
    private IReadOnlyList<IPartialGuild>? _cachedCurrentUserGuilds;

    public DashboardDiscordClient(IDiscordRestUserAPI userApi)
    {
        _userApi  = userApi;
    }

    public async Task<IUser?> GetCurrentUserAsync(bool forceRefresh = false)
    {
        if (_cachedUser is null || forceRefresh)
        {
            var result = await _userApi.GetCurrentUserAsync();
            _cachedUser = result.IsDefined(out var user) ? user : null;
        }
        
        return _cachedUser;
    }

    public async Task<IReadOnlyList<IPartialGuild>?> GetCurrentUserGuildsAsync(bool forceRefresh = false)
    {
        if (_cachedCurrentUserGuilds is null || forceRefresh)
        {
            var result = await _userApi.GetCurrentUserGuildsAsync(limit: 100);
            _cachedCurrentUserGuilds = result.IsDefined(out var guilds) ? guilds : null;
        }

        return _cachedCurrentUserGuilds;
    }

    public async Task<IReadOnlyList<IPartialGuild>?> GetCurrentUserGuildsByPermissionsAsync
    (
        DiscordPermission permissions,
        bool              forceRefresh = false
    )
    {
        return FilterGuildsByPermission(await GetCurrentUserGuildsAsync(forceRefresh), permissions);
    }

    public IReadOnlyList<IPartialGuild>? FilterGuildsByPermission
    (
        IReadOnlyList<IPartialGuild>? guilds,
        DiscordPermission             permission
    )
    {
        return guilds?.Where(g => g.Permissions.IsDefined(out var permissionSet) && 
                                  permissionSet.HasPermission(permission))
                      .ToList();
    }

    public async Task<IPartialGuild?> GetCurrentUserGuildByIdAndPermissionAsync
    (
        Snowflake         guildId,
        DiscordPermission permission,
        bool              forceRefresh = false
    )
    {
        var cachedGuilds = await GetCurrentUserGuildsAsync(forceRefresh);
        var guild = cachedGuilds?.FirstOrDefault(guild => guild.ID.Value == guildId                          &&
                                                          guild.Permissions.IsDefined(out var permissionSet) &&
                                                          permissionSet.HasPermission(permission));
        return guild;
    }
}