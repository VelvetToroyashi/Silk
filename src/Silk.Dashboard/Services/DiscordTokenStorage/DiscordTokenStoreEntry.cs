using Silk.Dashboard.Services.DiscordTokenStorage.Interfaces;

namespace Silk.Dashboard.Services.DiscordTokenStorage;

// To be used as Singleton in DI

public record DiscordTokenStoreEntry
(
    string?         AccessToken,
    string?         RefreshToken,
    DateTimeOffset? ExpiresAt,
    string?         TokenType
) : IDiscordTokenStoreEntry;