#nullable enable

using System.Collections.Concurrent;

namespace Silk.Dashboard.Services;

// To be used as Singleton in DI
public interface IDiscordTokenStore
{
    bool                     SetToken(string    userId, IDiscordTokenStoreEntry? token);
    IDiscordTokenStoreEntry? GetToken(string    userId);
    bool                     RemoveToken(string userId);
    void                     RemoveAllTokens();
}

public interface IDiscordTokenStoreEntry
{
    // Todo: Protect Access and Refresh tokens?
    public string?         AccessToken  { get; }
    public string?         RefreshToken { get; }
    public DateTimeOffset? ExpiresAt    { get; }
    public string?         TokenType    { get; }
}

public record DiscordTokenStoreEntry
(
    string?         AccessToken,
    string?         RefreshToken,
    DateTimeOffset? ExpiresAt,
    string?         TokenType
) : IDiscordTokenStoreEntry;

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

public static class DiscordTokenStoreExtensions
{
    public static DateTimeOffset? GetTokenExpiry(string? value)
    {
        return DateTimeOffset.TryParse(value, out var tokenExpiry)
            ? tokenExpiry
            : null;
    }
}