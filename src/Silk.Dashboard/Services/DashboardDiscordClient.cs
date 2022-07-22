#nullable enable

using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Caching.Abstractions.Services;
using Remora.Discord.Rest.Extensions;
using Remora.Rest;
using Remora.Rest.Core;

namespace Silk.Dashboard.Services;

public class DashboardDiscordClient
{
    private readonly ICacheProvider    _cache;
    private readonly DiscordTokenStore _tokenStore;
    private readonly IRestHttpClient   _restHttpClient;

    public DashboardDiscordClient
    (
        ICacheProvider    cache,
        DiscordTokenStore tokenStore,
        IRestHttpClient   restHttpClient
    )
    {
        _cache          = cache;
        _tokenStore     = tokenStore;
        _restHttpClient = restHttpClient;

        _restHttpClient.WithCustomization
        (
             b => b.WithRateLimitContext(_cache)
                    .With(m => m.Headers.Authorization = new("Bearer", GetCurrentUserToken()))
        );
    }

    private string? GetCurrentUserToken() 
        => _tokenStore.GetToken(_tokenStore.CurrentUserId)?.AccessToken;

    public async Task<IUser?> GetCurrentUserAsync()
    {
        var result = await _restHttpClient.GetAsync<IUser>("users/@me");
        return result.IsDefined(out var user) ? user : null;
    }

    public async Task<IReadOnlyList<IPartialGuild>?> GetCurrentUserGuildsAsync()
    {
        const uint limit = 100;
        var result = await _restHttpClient.GetAsync<IReadOnlyList<IPartialGuild>>
        (
             "users/@me/guilds",
             b => b.AddQueryParameter("limit", limit.ToString())
        );

        return result.IsDefined(out var guilds) ? guilds : null;
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