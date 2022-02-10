using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Rest.Core;

namespace Silk.Dashboard.Services;

public class DiscordRestClientService
{
    public IDiscordRestUserAPI RestClient { get; }

    private IReadOnlyList<IPartialGuild> _cachedCurrentUserGuilds;

    public DiscordRestClientService(IDiscordRestUserAPI  restClient)
    {
        RestClient = restClient;
    }

    public async Task<IReadOnlyList<IPartialGuild>> GetAllGuildsAsync()
    {
        var guilds = (await RestClient.GetCurrentUserGuildsAsync(limit: 100)).Entity;
        _cachedCurrentUserGuilds = guilds;

        return guilds;
    }

    public async Task<IReadOnlyList<IPartialGuild>> GetGuildsByPermissionAsync(DiscordPermission permissions)
    {
        return FilterGuildsByPermission(await GetAllGuildsAsync(), permissions);
    }

    public IReadOnlyList<IPartialGuild> FilterGuildsByPermission(IReadOnlyList<IPartialGuild> guilds,
                                                                 DiscordPermission            permission)
    {
        return guilds.Where(g => g.Permissions.IsDefined(out var permissionSet) &&
                                 permissionSet.HasPermission(permission)).ToList();
    }

    private async Task<IReadOnlyList<IPartialGuild>> GetCachedCurrentUserGuilds(bool forceRefresh = false)
    {
        if (_cachedCurrentUserGuilds is null || forceRefresh)
            _cachedCurrentUserGuilds = await GetAllGuildsAsync();

        return _cachedCurrentUserGuilds;
    }

    public async Task<IPartialGuild?> GetGuildByIdAndPermissionAsync(Snowflake         guildId,
                                                               DiscordPermission permission)
    {
        var cachedGuilds = await GetCachedCurrentUserGuilds();
        var guild = cachedGuilds.FirstOrDefault(guild => guild.ID.Value == guildId &&
                                                                     guild.Permissions.IsDefined(out var permissionSet) &&
                                                                     permissionSet.HasPermission(permission));
        return guild;
    }
}