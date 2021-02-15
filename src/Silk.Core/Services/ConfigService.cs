using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Silk.Core.Database.Models;
using Silk.Core.Services.Interfaces;

namespace Silk.Core.Services
{
    public class ConfigService
    {
        private readonly IMemoryCache _cache;
        private readonly IDatabaseService _db;
        public ConfigService(IMemoryCache cache, IDatabaseService db)
        {
            _cache = cache;
            _db = db;
        }

        public async ValueTask<GuildConfig> GetConfigAsync(ulong guildId)
        {
            if (_cache.TryGetValue(guildId, out GuildConfig config)) return config;
            return await GetConfigFromDatabaseAsync(guildId);
        }

        private async Task<GuildConfig> GetConfigFromDatabaseAsync(ulong guildId)
        {
            GuildConfig configuration = await _db.GetConfigAsync(guildId);
            _cache.Set(guildId, configuration, TimeSpan.FromHours(1));
            return configuration;
        }
    }
}