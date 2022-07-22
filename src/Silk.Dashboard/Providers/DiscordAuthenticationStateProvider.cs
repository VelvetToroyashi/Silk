using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Silk.Dashboard.Extensions;
using Silk.Dashboard.Services;

namespace Silk.Dashboard.Providers;

public class DiscordAuthenticationStateProvider : RevalidatingServerAuthenticationStateProvider
{
    // Adjust this parameter to control the time after which the authentication state will be revalidated.
    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(3);

    private readonly DiscordTokenStore _tokenStore;

    public DiscordAuthenticationStateProvider
    (
        DiscordTokenStore tokenStore,
        ILoggerFactory     loggerFactory
    ) : base(loggerFactory)
    {
        _tokenStore = tokenStore;
    }

    protected override Task<bool> ValidateAuthenticationStateAsync
    (
        AuthenticationState authState,
        CancellationToken   cancellationToken
    )
    {
        if (!authState.User.IsAuthenticated())
            return Task.FromResult(false);

        var tokenStoreEntry = _tokenStore.GetToken(_tokenStore.CurrentUserId);
        return tokenStoreEntry is null 
            ? Task.FromResult(false) 
            : Task.FromResult(tokenStoreEntry.ExpiresAt >= DateTimeOffset.UtcNow);
    }
}