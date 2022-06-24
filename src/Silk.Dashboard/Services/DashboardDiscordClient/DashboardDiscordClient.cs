#nullable enable

using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Caching.Abstractions.Services;
using Remora.Discord.Rest.Extensions;
using Remora.Rest;
using Remora.Rest.Core;
using Silk.Dashboard.Services.DiscordTokenStorage.Interfaces;

namespace Silk.Dashboard.Services.DashboardDiscordClient;

public class DashboardDiscordClient
{
    private readonly IRestHttpClient       _restHttpClient;
    private readonly IDiscordTokenStore    _tokenStore;
    private readonly ICacheProvider        _cacheProvider;
    
    private readonly IDiscordRestUserAPI   _userApi;
    private readonly IDiscordRestOAuth2API _oAuth2Api;

    public DashboardDiscordClient(
        IRestHttpClient restHttpClient,
        IDiscordTokenStore tokenStore,
        ICacheProvider cacheProvider,
        IDiscordRestUserAPI   userApi,
        IDiscordRestOAuth2API oAuth2Api)
    {
        _restHttpClient = restHttpClient;
        _tokenStore     = tokenStore;
        _cacheProvider  = cacheProvider;
        _userApi        = userApi;
        _oAuth2Api      = oAuth2Api;

        _restHttpClient.WithCustomization
        (
         b =>
             {
                 b.WithRateLimitContext(_cacheProvider)
                  .With(m => m.Headers.Authorization = new("Bearer", GetCurrentUserToken()));
             }
        );
    }

    private string? GetCurrentUserToken() => _tokenStore.GetToken(_tokenStore.CurrentUserId)?.AccessToken;

    public async Task<IUser?> GetCurrentUserAsync()
    {
        var result = await _restHttpClient.GetAsync<IUser>("users/@me");
        return result.IsDefined(out var user) ? user : null;
    }

    public async Task<IReadOnlyList<IPartialGuild>?> GetCurrentUserGuildsAsync()
    {
        var result = await _restHttpClient.GetAsync<IReadOnlyList<IPartialGuild>>
        (
         "users/@me/guilds",
         b => b.AddQueryParameter("limit", 100.ToString())
        );

        return result.IsDefined(out var guilds) ? guilds : null;
    }

    public async Task<IReadOnlyList<IPartialGuild>?> GetCurrentUserGuildsByPermissionAsync
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
        return guilds?.Where(g => g.Permissions.IsDefined(out var permissionSet) && 
                                  permissionSet.HasPermission(permission))
                      .ToList();
    }

    public async Task<IPartialGuild?> GetCurrentUserGuildByIdAndPermissionAsync
    (
        Snowflake         guildId,
        DiscordPermission permission
    )
    {
        var userGuilds = await GetCurrentUserGuildsAsync();
        var guild = userGuilds?.FirstOrDefault
        (
             guild => guild.ID.Value == guildId &&
                      guild.Permissions.IsDefined(out var permissionSet) &&
                      permissionSet.HasPermission(permission)
        );
        return guild;
    }
    
    public async Task<IApplication?> GetCurrentBotApplicationInformationAsync()
    {
        var result = await _oAuth2Api.GetCurrentBotApplicationInformationAsync();
        return result.IsDefined(out var application) ? application : null;
    }

    public async Task<IAuthorizationInformation?> GetCurrentAuthorizationInformationAsync()
    {
        var result = await _oAuth2Api.GetCurrentAuthorizationInformationAsync();
        return result.IsDefined(out var authorizationInformation) ? authorizationInformation : null;
    }
}