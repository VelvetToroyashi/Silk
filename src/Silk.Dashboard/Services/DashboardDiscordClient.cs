#nullable enable

using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Caching.Abstractions.Services;
using Remora.Discord.Rest.Extensions;
using Remora.Rest;
using Remora.Rest.Core;

namespace Silk.Dashboard.Services;

public class DashboardDiscordClient
{
    private const string UserGuildsKey = "dashboard:user-guilds";
    private const string BotGuildsKey  = "dashboard:bot-guilds";

    private const string ChannelsKey   = "dashboard:bot-guild-channels";
    private const string RolesKey      = "dashboard:bot-guild-roles";

    private readonly ICacheProvider       _cache;
    private readonly IDiscordRestUserAPI  _userApi;
    private readonly IDiscordRestGuildAPI _guildApi;
    private readonly DiscordTokenStore    _tokenStore;
    private readonly IRestHttpClient      _restHttpClient;

    public DashboardDiscordClient
    (
        ICacheProvider      cache,
        DiscordTokenStore   tokenStore,
        IRestHttpClient     restHttpClient,
        IDiscordRestUserAPI userApi,
        IDiscordRestGuildAPI guildApi
    )
    {
        _cache          = cache;
        _tokenStore     = tokenStore;
        _restHttpClient = restHttpClient;
        _userApi        = userApi;
        _guildApi  = guildApi;

        _restHttpClient.WithCustomization
        (
             b => b.WithRateLimitContext(_cache)
                    .With(m => m.Headers.Authorization = new("Bearer", GetCurrentUserToken()))
        );
    }

    private string? GetCurrentUserToken() 
        => _tokenStore.GetToken(_tokenStore.CurrentUserId)?.AccessToken;

    public async Task<Dictionary<Snowflake, IPartialGuild>?> GetBotGuildsAsync()
    {
        IReadOnlyList<IPartialGuild>? botGuilds;

        var result = await _cache.RetrieveAsync<IReadOnlyList<IPartialGuild>>(BotGuildsKey);
        if (result.IsDefined(out botGuilds))
            return botGuilds.ToDictionary(g => g.ID.Value, g => g);

        result = await _userApi.GetCurrentUserGuildsAsync();
        if (!result.IsDefined(out botGuilds))
            return null;

        await _cache.CacheAsync
        (
             BotGuildsKey,
             botGuilds,
             absoluteExpiration: DateTimeOffset.UtcNow.AddMinutes(1)
        );

        return botGuilds.ToDictionary(g => g.ID.Value, g => g);
    }

    // TODO: Make a DTO + refactor.
    public async Task<IReadOnlyList<IChannel>?> GetBotChannelsAsync(Snowflake guildId)
    {
        IReadOnlyList<IChannel>? guildChannels;

        var cacheKey = $"{ChannelsKey}:{guildId.Value}";

        var result = await _cache.RetrieveAsync<IReadOnlyList<IChannel>>(cacheKey);
        if (result.IsDefined(out guildChannels))
            return guildChannels;

        result = await _guildApi.GetGuildChannelsAsync(guildId);
        if (!result.IsDefined(out guildChannels))
            return null;

        guildChannels = guildChannels.Where(c => c.Type == ChannelType.GuildText)
                                     .ToList();

        await _cache.CacheAsync
        (
             cacheKey,
             guildChannels,
             absoluteExpiration: DateTimeOffset.UtcNow.AddMinutes(1)
        );

        return guildChannels;
    }

    // TODO: Make a DTO + refactor.
    public async Task<IReadOnlyList<IRole>?> GetBotRolesAsync(Snowflake guildId)
    {
        IReadOnlyList<IRole>? guildRoles;

        var cacheKey = $"{RolesKey}:{guildId.Value}";
        
        var result   = await _cache.RetrieveAsync<IReadOnlyList<IRole>>(cacheKey);
        if (result.IsDefined(out guildRoles))
            return guildRoles;

        result = await _guildApi.GetGuildRolesAsync(guildId);
        if (!result.IsDefined(out guildRoles))
            return null;

        await _cache.CacheAsync
        (
             cacheKey,
             guildRoles,
             absoluteExpiration: DateTimeOffset.UtcNow.AddMinutes(1)
        );

        return guildRoles;
    }

    public async Task<IReadOnlyList<IPartialGuild>?> GetCurrentUserBotManagedGuildsAsync
    (
        IReadOnlyList<IPartialGuild>? guilds = null
    )
    {
        IReadOnlyList<IPartialGuild>? managedGuilds = null;

        var botGuilds  = await GetBotGuildsAsync();
        var userGuilds = FilterGuildsByPermission
        (
             guilds ?? await GetCurrentUserGuildsAsync(),
             DiscordPermission.ManageGuild
        );

        if (botGuilds is not null && userGuilds is not null)
            managedGuilds = userGuilds.Where(g => botGuilds.ContainsKey(g.ID.Value)).ToList();

        return managedGuilds;
    }

    public async Task<IUser?> GetCurrentUserAsync()
    {
        var result = await _restHttpClient.GetAsync<IUser>("users/@me");
        return result.IsDefined(out var user) ? user : null;
    }

    // TODO: Use cache.
    public async Task<IReadOnlyList<IPartialGuild>?> GetCurrentUserGuildsAsync()
    {
        const uint limit = 100;

        IReadOnlyList<IPartialGuild>? guilds;

        var result = await _cache.RetrieveAsync<IReadOnlyList<IPartialGuild>>(UserGuildsKey);
        if (result.IsDefined(out guilds))
            return guilds;

        result = await _restHttpClient.GetAsync<IReadOnlyList<IPartialGuild>>
        (
         "users/@me/guilds",
         b => b.AddQueryParameter("limit", limit.ToString())
        );

        if (!result.IsDefined(out guilds))
            return null;

        await _cache.CacheAsync
        (
             UserGuildsKey,
             guilds,
             absoluteExpiration: DateTimeOffset.UtcNow.AddMinutes(1)
        );

        return guilds;
    }

    public async Task<IReadOnlyList<IPartialGuild>?> GetCurrentUserGuildsAsync
    (
        DiscordPermission permission
    )
    {
        return FilterGuildsByPermission(await GetCurrentUserGuildsAsync(), permission);
    }

    public IReadOnlyList<IPartialGuild>? FilterGuildsByPermission
    (
        IReadOnlyList<IPartialGuild>? guilds,
        DiscordPermission             permission
    )
    {
        return guilds?.Where(guild => GuildHasPermission(guild, permission))
                      .ToList();
    }

    public async Task<IPartialGuild?> GetCurrentUserGuildAsync
    (
        Snowflake         guildId,
        DiscordPermission permission
    )
    {
        var userGuilds = await GetCurrentUserGuildsAsync();
        var guild = userGuilds?.FirstOrDefault
        (
             guild => guild.ID.IsDefined(out var gID) &&
                      gID == guildId &&
                      GuildHasPermission(guild, permission)
        );

        return guild;
    }

    private static bool GuildHasPermission
    (
        IPartialGuild     guild,
        DiscordPermission permission
    )
    {
        return guild.Permissions.IsDefined(out var permissions) &&
               permissions.HasPermission(permission);
    }
}