using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Silk.Core.Database;
using Silk.Core.Database.Models;
using Silk.Core.Services.Interfaces;

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
            if (_cache.TryGetValue(guildId.Value, out string? prefix)) return prefix!;
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
            _cache.TryAdd(guild.Id, guild.Prefix);
            //_memoryCache.Set(guildId, guild.Prefix, DateTimeOffset.UtcNow.AddSeconds(1));
            // _memoryCache
            //     .CreateEntry(guildId)
            //     .SetValue(guild.Prefix)
            //     .SetSlidingExpiration(TimeSpan.FromMinutes(10))
            //     //.SetAbsoluteExpiration(DateTimeOffset.UtcNow.AddMinutes(10))
            //     .RegisterPostEvictionCallback((key, _, reason, _) => _logger.LogInformation($"{key} was evicted from cache for {reason}"));
            //
            _logger.LogDebug($"Cached {guild.Prefix} - {guildId} in {_sw.ElapsedMilliseconds} ms");

            return guild.Prefix;
        }

        public void UpdatePrefix(ulong id, string prefix)
        {
            _cache.TryGetValue(id, out string? currentPrefix);
            _cache.AddOrUpdate(id, prefix, (_, _) => prefix);
            _logger.LogDebug($"Updated prefix for {id} - {currentPrefix} -> {prefix}");
        }
        public void PurgeCache(ulong id)
        {
            _cache.TryRemove(id, out _);
            _ = GetPrefixFromDatabase(id);
            _logger.LogDebug("Purged prefix from recached from database");
        }
    }
}