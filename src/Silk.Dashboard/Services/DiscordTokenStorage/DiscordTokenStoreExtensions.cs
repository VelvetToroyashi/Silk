#nullable enable

using Microsoft.AspNetCore.Authentication.OAuth;

namespace Silk.Dashboard.Services.DiscordTokenStorage;

public static class DiscordTokenStoreExtensions
{
    private const string DiscordAuthenticationTokenExpiryKey = ".Token.expires_at";

    public static DateTimeOffset? GetTokenExpiry(string? value)
    {
        return DateTimeOffset.TryParse(value, out var tokenExpiry)
            ? tokenExpiry
            : null;
    }
    
    public static DateTimeOffset? GetTokenExpiry(this OAuthCreatingTicketContext context)
    {
        return GetTokenExpiry(context.Properties.Items[DiscordAuthenticationTokenExpiryKey]);
    }
}