using Silk.Dashboard.Models;

namespace Silk.Dashboard.Services
{
    public class DashboardTokenStorageService : IDashboardTokenStorageService
    {
        private DiscordOAuthToken? _token;

        public void SetToken(DiscordOAuthToken? token) => _token = token;
        public DiscordOAuthToken? GetToken() => _token;
        public void ClearToken() => _token = null;
    }
}