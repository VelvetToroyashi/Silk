using Silk.Core.Services.Interfaces;

namespace Silk.Core.Services.Data
{
    public sealed class CacheUpdaterService : ICacheUpdaterService
    {
        public event GuildConfigUpdated? ConfigUpdated;

        public void UpdateGuild(ulong id)
        {
            ConfigUpdated?.Invoke(id);
        }
    }
}