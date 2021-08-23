using Silk.Dashboard.Models;

namespace Silk.Dashboard.Services
{
    public interface IDashboardTokenStorageService
    {
        void SetToken(DiscordOAuthToken? token);
        DiscordOAuthToken? GetToken();
        void ClearToken();
    }
}