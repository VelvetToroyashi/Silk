#nullable enable

using Microsoft.AspNetCore.Authentication.OAuth;
using Silk.Dashboard.Services.DiscordTokenStorage.Interfaces;

namespace Silk.Dashboard.Services.DiscordTokenStorage;

// To be used as Singleton in DI

public record DiscordTokenStoreEntry
(
    string?         AccessToken,
    string?         RefreshToken,
    string?         TokenType,
    DateTimeOffset? ExpiresAt
) : IDiscordTokenStoreEntry
{
    public DiscordTokenStoreEntry
    (
        OAuthCreatingTicketContext context
    ) : this
        (
         context.AccessToken,
         context.RefreshToken,
         context.TokenType,
         context.GetTokenExpiry()
        )
    {
    }
}