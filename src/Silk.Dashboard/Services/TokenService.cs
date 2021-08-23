#nullable enable
using Silk.Dashboard.Models;

namespace Silk.Dashboard.Services
{
    public interface ITokenService
    {
        public DiscordOAuthTokenResponse? Token { get; }
        void SetToken(DiscordOAuthTokenResponse? token);
        void ClearToken();
    }

    public class TokenService : ITokenService
    {
        public DiscordOAuthTokenResponse? Token { get; private set; }
        public void SetToken(DiscordOAuthTokenResponse? token) => Token = token;
        public void ClearToken() => Token = null;
    }
}