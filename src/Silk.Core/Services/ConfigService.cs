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

        public ConfigService(IMemoryCache cache, IDatabaseService db) => (_cache, _db) = (cache, db);

        public async ValueTask<GuildConfigModel> GetConfigAsync(ulong guildId)
        {
            if (_cache.TryGetValue(guildId, out GuildConfigModel config)) return config;
            return await GetConfigFromDatabaseAsync(guildId);
        }

        private async Task<GuildConfigModel> GetConfigFromDatabaseAsync(ulong guildId)
        {
            GuildConfigModel configuration = await _db.GetConfigAsync(guildId);
            _cache.Set(guildId, configuration, TimeSpan.FromHours(1));
            return configuration;
        }
    }
}