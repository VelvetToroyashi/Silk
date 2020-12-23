#region

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Silk.Core.Database;
using Silk.Core.Database.Models;

#endregion

namespace Silk.Core.Services
{
    public class GuildConfigCacheService
    {
        private readonly IMemoryCache _cache;
        private readonly DatabaseService _db;
        private readonly ILogger<GuildConfigCacheService> _logger;

        public GuildConfigCacheService(IMemoryCache cache, DatabaseService db,
            ILogger<GuildConfigCacheService> logger)
        {
            _cache = cache;
            _db = db;
            _logger = logger;
        }

        public async Task<GuildConfigModel> GetConfigAsync(ulong? guildId)
        {
            if (guildId is null || guildId == 0) return default;
            if (_cache.TryGetValue(guildId.Value, out GuildConfigModel config)) return config;
            return await GetConfigFromDatabaseAsync(guildId.Value);
        }

        public async Task<GuildConfigModel> GetConfigFromDatabaseAsync(ulong guildId)
        {

            GuildModel config = await _db.GetGuildAsync(guildId);
            _cache.CreateEntry(guildId).SetValue(config.Configuration)
                  .SetPriority(CacheItemPriority.Low); // Expires in 1 hour if not accessed. //
            return config.Configuration;
        }
    }
}