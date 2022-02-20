using System.Collections.Concurrent;
using Silk.Dashboard.Services.DiscordTokenStorage.Interfaces;

namespace Silk.Dashboard.Services.DiscordTokenStorage;

public class DiscordTokenStore : IDiscordTokenStore
{
    private readonly ConcurrentDictionary<string, IDiscordTokenStoreEntry> _tokenStore = new();

    public bool SetToken(string userId, IDiscordTokenStoreEntry? token)
    {
        if (token is null) return false;
        _tokenStore.AddOrUpdate(userId, _ => token, (_, _) => token);
        return true;
    }

    public IDiscordTokenStoreEntry? GetToken(string userId)
    {
        _tokenStore.TryGetValue(userId, out var tokenEntry);
        return tokenEntry;
    }

    public bool RemoveToken(string userId)
    {
        return _tokenStore.TryRemove(userId, out var tokenEntry);
    }

    public void RemoveAllTokens()
    {
        _tokenStore.Clear();
    }
}