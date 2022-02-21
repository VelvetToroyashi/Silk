#nullable enable

namespace Silk.Dashboard.Services.DiscordTokenStorage.Interfaces;

// Registered as Singleton in DI
public interface IDiscordTokenStore
{
    bool                     SetToken(string    userId, IDiscordTokenStoreEntry? token);
    IDiscordTokenStoreEntry? GetToken(string    userId);
    bool                     RemoveToken(string userId);
    void                     RemoveAllTokens();
    IReadOnlyDictionary<string, IDiscordTokenStoreEntry> GetEntries();
}