#nullable enable

using System.Collections.Concurrent;
using Silk.Dashboard.Services.DiscordTokenStorage.Interfaces;

namespace Silk.Dashboard.Services.DiscordTokenStorage;

public class DiscordTokenStore : IDiscordTokenStore
{
    private readonly ConcurrentDictionary<string, IDiscordTokenStoreEntry> _tokenStore = new();
    
    private string? _currentUserId;
    public string? CurrentUserId => _currentUserId;

    public bool SetToken(string userId, IDiscordTokenStoreEntry? token)
    {
        if (token is null) return false;
        _tokenStore.AddOrUpdate(userId, _ =>
        {
            _currentUserId = userId;
            return token;
        }, (_, _) => token);
        return true;
    }

    public IDiscordTokenStoreEntry? GetToken(string? userId)
    {
        if (string.IsNullOrEmpty(userId)) return null;
        _tokenStore.TryGetValue(userId, out var tokenEntry);
        return tokenEntry;
    }

    public bool RemoveToken(string? userId)
    {
        if (string.IsNullOrEmpty(userId) || 
            !_tokenStore.TryRemove(userId, out _)) return false;
        
        if (userId == _currentUserId) _currentUserId = null;
        return true;
    }

    public void RemoveAllTokens()
    {
        _tokenStore.Clear();
        _currentUserId = null;
    }

    public IReadOnlyDictionary<string, IDiscordTokenStoreEntry> GetEntries()
    {
        return _tokenStore;
    }
}