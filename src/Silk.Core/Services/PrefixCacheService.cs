using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Silk.Core.Services.Interfaces;
using Silk.Data;
using Silk.Data.Models;

namespace Silk.Core.Services
{
    /// <inheritdoc cref="IPrefixCacheService"/>
    public class PrefixCacheService : IPrefixCacheService
    {
        private readonly ILogger _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly ConcurrentDictionary<ulong, string> _cache = new();
        private readonly IDbContextFactory<SilkDbContext> _dbFactory;
        private readonly Stopwatch _sw = new();
        private readonly IServiceCacheUpdaterService _cacheUpdater;
        public PrefixCacheService(ILogger<PrefixCacheService> logger, IDbContextFactory<SilkDbContext> dbFactory, IMemoryCache memoryCache, IServiceCacheUpdaterService cacheUpdater)
        {
            _logger = logger;
            _dbFactory = dbFactory;
            _memoryCache = memoryCache;
            _cacheUpdater = cacheUpdater;

            _cacheUpdater.ConfigUpdated += PurgeCache;
        }

        [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
        public string RetrievePrefix(ulong? guildId)
        {

            if (guildId == default || guildId == 0) return string.Empty;
            if (_memoryCache.TryGetValue(GetGuildString(guildId.Value), out var prefix)) return (string) prefix;
            return GetPrefixFromDatabase(guildId.Value);
        }

        private string GetPrefixFromDatabase(ulong guildId)
        {
            _sw.Restart();

            SilkDbContext db = _dbFactory.CreateDbContext();

            Guild? guild = db.Guilds.AsNoTracking().FirstOrDefault(g => g.Id == guildId);
            if (guild is null)
            {
                _logger.LogCritical("Guild was not cached on join, and therefore does not exist in database");
                return Bot.DefaultCommandPrefix;
            }

            _sw.Stop();
            _memoryCache.Set(GetGuildString(guildId), guild.Prefix);
            _logger.LogDebug($"Cached {guild.Prefix} - {guildId} in {_sw.ElapsedMilliseconds} ms");
            return guild.Prefix;
        }


        // I don't know if updating a reference will update 
        public void UpdatePrefix(ulong id, string prefix)
        {
            string key = GetGuildString(id);

            _memoryCache.TryGetValue(key, out string oldPrefix);
            _memoryCache.Set(key, prefix);
            _logger.LogDebug($"Updated prefix for {id} - {oldPrefix} -> {prefix}");
        }

        private static string GetGuildString(ulong id) => $"GUILD_PREFIX_KEY_{id}";

        public void PurgeCache(ulong id)
        {
            _cache.TryRemove(id, out _);
            //GetPrefix caches, so no need for the result.//
            _ = GetPrefixFromDatabase(id);
            _logger.LogDebug("Purged prefix from recached from database");
        }
    }
}