#nullable enable

using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Caching.Abstractions.Services;
using Remora.Discord.Rest.Extensions;
using Remora.Rest;
using Remora.Rest.Core;
using Remora.Results;

namespace Silk.Dashboard.Services;

public class DashboardDiscordClient
{
    private const string UserMeKey     = "dashboard:user-me";
    private const string UserGuildsKey = "dashboard:user-guilds";
    private const string BotGuildsKey  = "dashboard:bot-guilds";
    private const string RolesKey      = "dashboard:bot-guild-roles";
    private const string ChannelsKey   = "dashboard:bot-guild-channels";

    private readonly ICacheProvider       _cache;
    private readonly IDiscordRestUserAPI  _userApi;
    private readonly IDiscordRestGuildAPI _guildApi;
    private readonly DiscordTokenStore    _tokenStore;
    private readonly IRestHttpClient      _restHttpClient;

    public DashboardDiscordClient
    (
        ICacheProvider       cache,
        DiscordTokenStore    tokenStore,
        IRestHttpClient      restHttpClient,
        IDiscordRestUserAPI  userApi,
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
        var result = await CacheAsync
        (
             BotGuildsKey,
             () => _userApi.GetCurrentUserGuildsAsync(),
             DateTimeOffset.UtcNow.AddMinutes(4)
        );
        return result?.ToDictionary(g => g.ID.Value, g => g);
    }

    // TODO: Make a DTO
    public async Task<IReadOnlyList<IChannel>?> GetBotChannelsAsync(Snowflake guildId)
    {
        var result = await CacheAsync
        (
             $"{ChannelsKey}:{guildId.Value}",
             () => _guildApi.GetGuildChannelsAsync(guildId),
             DateTimeOffset.UtcNow.AddMinutes(4),
             channels => channels.Where(c => c.Type == ChannelType.GuildText).ToList()
        );
        return result;
    }

    public async Task<IReadOnlyList<IRole>?> GetBotRolesAsync(Snowflake guildId)
    {
        var result = await CacheAsync
        (
             $"{RolesKey}:{guildId.Value}",
             () => _guildApi.GetGuildRolesAsync(guildId),
             DateTimeOffset.UtcNow.AddMinutes(4),
             roles => roles.Where(r => r.ID != guildId && !r.IsManaged).ToList()
        );
        return result;
    }

    private async Task<T?> CacheAsync<T>
    (
        string                key,
        Func<Task<Result<T>>> func,
        DateTimeOffset        expiration,
        Func<T, T>?           modify = null 
    ) where T : class
    {
        var result = await _cache.RetrieveAsync<T>(key);
        if (result.IsDefined(out var ret))
            return ret;

        result = await func();
        if (!result.IsDefined(out ret))
            return null;
        
        if (modify is not null)
            ret = modify(ret);

        await _cache.CacheAsync(key, ret, expiration);

        return ret;
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

    public async Task<IPartialGuild?> GetCurrentUserBotManagedGuildAsync(Snowflake guildId)
    {
        var result = await GetCurrentUserBotManagedGuildsAsync();
        return result?.FirstOrDefault(g => g.ID.Value == guildId);
    }

    public async Task<IUser?> GetCurrentUserAsync()
    {
        var result = await CacheAsync
        (
             UserMeKey,
             () => _restHttpClient.GetAsync<IUser>("users/@me"),
             DateTimeOffset.UtcNow.AddMinutes(4)
        );
        return result;
    }

    public async Task<IReadOnlyList<IPartialGuild>?> GetCurrentUserGuildsAsync()
    {
        var result = await CacheAsync
        (
             UserGuildsKey,
             () => _restHttpClient.GetAsync<IReadOnlyList<IPartialGuild>>
             (
                  "users/@me/guilds",
                  b => b.AddQueryParameter("limit", "100")
             ),
             DateTimeOffset.UtcNow.AddMinutes(4)
        );
        return result;
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