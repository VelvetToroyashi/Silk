#nullable enable

namespace Silk.Dashboard.Services.DiscordTokenStorage.Interfaces;

public interface IDiscordTokenStore
{
    public string? CurrentUserId { get; }
    bool                     SetToken(string  userId, IDiscordTokenStoreEntry? token);
    IDiscordTokenStoreEntry? GetToken(string? userId);
    bool                     RemoveToken(string? userId);
    void                     RemoveAllTokens();
    IReadOnlyDictionary<string, IDiscordTokenStoreEntry> GetEntries();
}