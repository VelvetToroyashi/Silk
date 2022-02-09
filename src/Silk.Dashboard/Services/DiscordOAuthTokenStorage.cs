using Microsoft.AspNetCore.DataProtection;

namespace Silk.Dashboard.Services;

// To be used as Singleton in DI
public interface IDiscordOAuthTokenStorage
{
    void   SetAccessToken(string token);
    string GetAccessToken();
    void   ClearAccessToken();
}

public class DiscordOAuthTokenStorage : IDiscordOAuthTokenStorage
{
    private          string         _accessToken;
    private readonly IDataProtector _protector;

    public DiscordOAuthTokenStorage(IDataProtectionProvider provider)
        => _protector = provider.CreateProtector("Discord");

    public void   SetAccessToken(string token) => _accessToken = _protector.Protect(token);
    public string GetAccessToken()             => _protector.Unprotect(_accessToken);
    public void   ClearAccessToken()           => _accessToken = null;
}