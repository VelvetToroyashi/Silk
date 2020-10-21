using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SilkBot.Models;
using System.Linq;

namespace SilkBot.Services
{
    public class PrefixCacheService
    {
        private readonly ILogger _logger;
        private readonly IMemoryCache _cache;
        private readonly IDbContextFactory<SilkDbContext> _dbFactory;
        public PrefixCacheService(ILogger<PrefixCacheService> logger, IMemoryCache cache, IDbContextFactory<SilkDbContext> dbFactory) 
        {
            _logger = logger;
            _cache = cache;
            _dbFactory = dbFactory;
        }
        public string RetrievePrefix(ulong guildId)
        {
            if(_cache.TryGetValue(guildId, out string prefix))
            {
                return prefix;
            }
            else
            {
                _logger.LogDebug("Prefix not present in cache; queuing from database.");
                var db = _dbFactory.CreateDbContext();
                Guild guild = db.Guilds.First(g => g.DiscordGuildId == guildId);
                return _cache.Set(guild.DiscordGuildId, guild.Prefix);
            }
        }
    }
}
