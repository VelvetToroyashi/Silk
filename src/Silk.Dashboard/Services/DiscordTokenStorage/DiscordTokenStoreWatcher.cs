#nullable enable

using Silk.Dashboard.Services.DiscordTokenStorage.Interfaces;

namespace Silk.Dashboard.Services.DiscordTokenStorage;

public class DiscordTokenStoreWatcher : IDiscordTokenStoreWatcher
{
    private          Timer?                     _timer;
    private readonly ILogger<DiscordTokenStore> _logger;
    private readonly IDiscordTokenStore         _tokenStore;

    private readonly TimeSpan _checkTokenExpiryPeriod = TimeSpan.FromSeconds(15);

    // Todo: Zero because HttpContext.User will still have token if removed early
    private readonly TimeSpan _periodBeforeTokenExpires = TimeSpan.Zero;

    public DiscordTokenStoreWatcher
    (
        IDiscordTokenStore         tokenStore,
        ILogger<DiscordTokenStore> logger
    )
    {
        _tokenStore = tokenStore;
        _logger     = logger;
    }

    private void CreateTimer()
    {
        _timer ??= new Timer(CheckForExpiredTokens, null, TimeSpan.Zero, _checkTokenExpiryPeriod);
    }

    private void DestroyTimer()
    {
        _timer?.Dispose();
    }

    private void CheckForExpiredTokens(object? state)
    {
        _logger.LogInformation("Checking for expired tokens");

        var expiredTokens = 0;

        foreach (var storeEntry in _tokenStore.GetEntries())
        {
            if (!TokenExpired(storeEntry.Value, _periodBeforeTokenExpires))
                continue;

            _tokenStore.RemoveToken(storeEntry.Key);
            ++expiredTokens;
        }

        if (expiredTokens > 0)
            _logger.LogInformation("Removed {TokensRemoved} expired tokens", expiredTokens);
    }

    private static bool TokenExpired
    (
        IDiscordTokenStoreEntry? storeEntry,
        TimeSpan                 periodBeforeExpiration
    )
    {
        if (storeEntry?.ExpiresAt is null) return false;
        return storeEntry.ExpiresAt >= DateTimeOffset.UtcNow - periodBeforeExpiration;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        CreateTimer();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        DestroyTimer();
        _tokenStore.RemoveAllTokens();
        return Task.CompletedTask;
    }
}