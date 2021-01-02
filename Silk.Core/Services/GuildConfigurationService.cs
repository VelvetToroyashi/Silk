using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Silk.Core.Database;
using Silk.Core.Database.Models;

namespace Silk.Core.Services
{
    public class ConfigService
    {
        private readonly IMemoryCache _cache;
        private readonly DatabaseService _db;

        public ConfigService(IMemoryCache cache, DatabaseService db) => (_cache, _db) = (cache, db);

        public async Task<GuildConfigModel> GetConfigAsync(ulong guildId)
        {
            if (_cache.TryGetValue(guildId, out GuildConfigModel config)) return config;
            return await GetConfigFromDatabaseAsync(guildId);
        }

        public async Task<GuildConfigModel> GetConfigFromDatabaseAsync(ulong guildId)
        {
            GuildConfigModel configuration = await _db.GetConfigAsync(guildId);
            _cache.Set(guildId, configuration, TimeSpan.FromHours(1));
            return configuration;
        }
    }
}