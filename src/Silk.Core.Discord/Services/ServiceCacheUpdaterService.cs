using Silk.Core.Discord.Services.Interfaces;

namespace Silk.Core.Discord.Services
{
    public class ServiceCacheUpdaterService : IServiceCacheUpdaterService
    {
        public event GuildConfigUpdated? ConfigUpdated;

        public void UpdateGuild(ulong id)
        {
            ConfigUpdated?.Invoke(id);
        }
    }
}