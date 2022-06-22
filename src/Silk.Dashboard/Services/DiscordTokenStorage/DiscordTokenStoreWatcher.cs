#nullable enable

using Silk.Dashboard.Services.DiscordTokenStorage.Interfaces;

namespace Silk.Dashboard.Services.DiscordTokenStorage;

public class DiscordTokenStoreWatcher : IDiscordTokenStoreWatcher
{
    private readonly PeriodicTimer              _timer;
    private readonly ILogger<DiscordTokenStore> _logger;
    private readonly IDiscordTokenStore         _tokenStore;

    private readonly TimeSpan _checkTokenExpiryPeriod = TimeSpan.FromMinutes(5);

    // Todo: Zero because HttpContext.User will still have token if removed early
    private readonly TimeSpan _periodBeforeTokenExpires = TimeSpan.Zero;

    public DiscordTokenStoreWatcher
    (
        ILogger<DiscordTokenStore> logger,
        IDiscordTokenStore         tokenStore
    )
    {
        _logger     = logger;
        _tokenStore = tokenStore;
        _timer      = new(_checkTokenExpiryPeriod);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = CheckForExpiredTokensAsync(cancellationToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer.Dispose();
        _tokenStore.RemoveAllTokens();
        return Task.CompletedTask;
    }

    private async Task CheckForExpiredTokensAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (await _timer.WaitForNextTickAsync(cancellationToken))
            {
                var expiredTokens = 0;

                foreach (var storeEntry in _tokenStore.GetEntries())
                {
                    if (!TokenExpired(storeEntry.Value)) continue;

                    _tokenStore.RemoveToken(storeEntry.Key);
                    ++expiredTokens;
                }

                if (expiredTokens > 0)
                    _logger.LogInformation("Removed {TokensRemoved} expired tokens", expiredTokens);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation
                (
                 "{TypeName} - Operation was cancelled in {MethodName}",
                 GetType().Name,
                 nameof(CheckForExpiredTokensAsync)
                );
        }
    }

    private bool TokenExpired
    (
        IDiscordTokenStoreEntry? storeEntry
    )
    {
        if (storeEntry?.ExpiresAt is null) return false;
        return storeEntry.ExpiresAt + _periodBeforeTokenExpires <= DateTimeOffset.UtcNow;
    }
}