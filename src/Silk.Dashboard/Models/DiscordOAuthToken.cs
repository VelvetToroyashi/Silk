#nullable enable

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Silk.Dashboard.Extensions;

namespace Silk.Dashboard.Models;

public record DiscordOAuthToken
(
    string?         AccessToken,
    string?         RefreshToken,
    string?         TokenType,
    DateTimeOffset? ExpiresAt
)
{
    public DiscordOAuthToken
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

    public static async Task<DiscordOAuthToken> FromHttpContext(HttpContext context)
    {
        var results = await Task.WhenAll
        (
             context.GetTokenAsync("access_token"),
             context.GetTokenAsync("refresh_token"),
             context.GetTokenAsync("token_type"),
             context.GetTokenAsync("expires_at")
        );

        return new DiscordOAuthToken
        (
             results[0],
             results[1],
             results[2],
             DiscordTokenStoreExtensions.GetTokenExpiry(results[3])
        );
    }
}