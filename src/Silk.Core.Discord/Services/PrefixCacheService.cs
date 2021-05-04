using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Silk.Core.Data;
using Silk.Core.Data.Models;
using Silk.Core.Discord.Services.Interfaces;

namespace Silk.Core.Discord.Services
{
    /// <inheritdoc cref="IPrefixCacheService" />
    public class PrefixCacheService : IPrefixCacheService
    {
        private readonly ConcurrentDictionary<ulong, string> _cache = new();
        private readonly IDbContextFactory<GuildContext> _dbFactory;
        private readonly ILogger _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly Stopwatch _sw = new();
        public PrefixCacheService(ILogger<PrefixCacheService> logger, IDbContextFactory<GuildContext> dbFactory, IMemoryCache memoryCache, IServiceCacheUpdaterService cacheUpdater)
        {
            _logger = logger;
            _dbFactory = dbFactory;
            _memoryCache = memoryCache;

            cacheUpdater.ConfigUpdated += PurgeCache;
        }

        [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
        public string RetrievePrefix(ulong? guildId)
        {
            if (guildId == default || guildId == 0) return string.Empty;
            if (_memoryCache.TryGetValue(GetGuildString(guildId.Value), out var prefix)) return (string) prefix;
            return GetPrefixFromDatabase(guildId.Value);
        }


        // I don't know if updating a reference will update 
        public void UpdatePrefix(ulong id, string prefix)
        {
            string key = GetGuildString(id);

            _memoryCache.TryGetValue(key, out string oldPrefix);
            _memoryCache.Set(key, prefix);
            _logger.LogDebug($"Updated prefix for {id} - {oldPrefix} -> {prefix}");
        }

        public void PurgeCache(ulong id)
        {
            _cache.TryRemove(id, out _);
            //GetPrefix caches, so no need for the result.//
            _ = GetPrefixFromDatabase(id);
        }

        private string GetPrefixFromDatabase(ulong guildId)
        {
            _sw.Restart();

            GuildContext db = _dbFactory.CreateDbContext();

            Guild? guild = db.Guilds.AsNoTracking().FirstOrDefault(g => g.Id == guildId);
            if (guild is null)
            {
                _logger.LogCritical("Guild was not cached on join, and therefore does not exist in database");
                return Main.DefaultCommandPrefix;
            }
            _memoryCache.Set(GetGuildString(guildId), guild.Prefix);
            return guild.Prefix;
        }

        private static string GetGuildString(ulong id) => $"GUILD_PREFIX_KEY_{id}";
    }
}