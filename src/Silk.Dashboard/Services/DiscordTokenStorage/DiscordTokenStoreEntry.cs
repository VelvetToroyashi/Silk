#nullable enable

using Microsoft.AspNetCore.Authentication;
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

    public static async Task<DiscordTokenStoreEntry> FromHttpContext(HttpContext context)
    {
        var tasks = new[]
        {
            context.GetTokenAsync("access_token"),
            context.GetTokenAsync("refresh_token"),
            context.GetTokenAsync("token_type"),
            context.GetTokenAsync("expires_at"),
        };

        // Wait for all tokens to be retrieved
        await Task.WhenAll(tasks);

        return new DiscordTokenStoreEntry
        (
         tasks[0].Result,
         tasks[1].Result,
         tasks[2].Result,
         DiscordTokenStoreExtensions.GetTokenExpiry(tasks[3].Result)
        );
    }
}