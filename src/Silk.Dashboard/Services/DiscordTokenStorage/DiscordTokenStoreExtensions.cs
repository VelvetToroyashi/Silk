namespace Silk.Dashboard.Services.DiscordTokenStorage;

public static class DiscordTokenStoreExtensions
{
    public static DateTimeOffset? GetTokenExpiry(string? value)
    {
        return DateTimeOffset.TryParse(value, out var tokenExpiry)
            ? tokenExpiry
            : null;
    }
}