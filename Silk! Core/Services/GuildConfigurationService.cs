using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SilkBot.Models;
using System.Linq;

namespace SilkBot.Utilities
{
    public class GuildConfigCacheService
    {
        private readonly IMemoryCache _cache;
        private readonly IDbContextFactory<SilkDbContext> _dbFactory;
        public GuildConfigCacheService(IMemoryCache cache, IDbContextFactory<SilkDbContext> dbFactory)
        {
            _cache = cache;
            _dbFactory = dbFactory;
        }
        public bool RetrievePrefix(ulong guildId)
        {
            if (_cache.TryGetValue(guildId, out bool whitelist))
            {
                return whitelist;
            }
            else
            {
                var db = _dbFactory.CreateDbContext();
                Guild guild = db.Guilds.First(g => g.DiscordGuildId == guildId);
                _cache.CreateEntry(guild.DiscordGuildId).SetValue(guild.WhiteListInvites).SetPriority(CacheItemPriority.Low);
                return guild.WhiteListInvites;
            }
        }
    }
}
