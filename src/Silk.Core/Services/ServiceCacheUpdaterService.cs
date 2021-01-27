using Silk.Core.Services.Interfaces;

namespace Silk.Core.Services
{
    public class ServiceCacheUpdaterService : IServiceCacheUpdaterService
    {
        public event GuildConfigUpdated? ConfigUpdated;

        public void UpdateGuild(ulong id) => ConfigUpdated?.Invoke(id);
    }
}