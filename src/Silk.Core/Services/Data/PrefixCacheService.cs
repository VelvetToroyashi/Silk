using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Silk.Core.Data.MediatR.Guilds;
using Silk.Core.Data.Models;
using Silk.Core.Services.Interfaces;

namespace Silk.Core.Services.Data
{
    /// <inheritdoc cref="IPrefixCacheService" />
    public sealed class PrefixCacheService : IPrefixCacheService
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PrefixCacheService> _logger;
        private readonly IMemoryCache _memoryCache;
        public PrefixCacheService(ILogger<PrefixCacheService> logger, IMemoryCache memoryCache, ICacheUpdaterService cacheUpdater, IMediator mediator)
        {
            _logger = logger;
            _memoryCache = memoryCache;
            _mediator = mediator;

            cacheUpdater.ConfigUpdated += PurgeCache;
        }

        [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
        public string RetrievePrefix(ulong? guildId)
        {
            if (guildId is null or 0) return string.Empty;
            if (_memoryCache.TryGetValue(GetGuildString(guildId.Value), out var prefix)) return (string) prefix;
            return GetDatabasePrefixAsync(guildId.Value).GetAwaiter().GetResult();
        }

        // I don't know if updating a reference will update 
        public void UpdatePrefix(ulong id, string prefix)
        {
            string key = GetGuildString(id);

            _memoryCache.TryGetValue(key, out string oldPrefix);
            _memoryCache.Set(key, prefix);
            _logger.LogDebug($"Updated prefix for {id} - {oldPrefix} -> {prefix}");
        }

        public void PurgeCache(ulong id) => _ = GetDatabasePrefixAsync(id);

        private async Task<string> GetDatabasePrefixAsync(ulong guildId)
        {
            Guild guild = await _mediator.Send(new GetGuildRequest(guildId));
            _memoryCache.Set(GetGuildString(guildId), guild.Prefix);
            return guild.Prefix;
        }

        private static string GetGuildString(ulong id) => $"GUILD_PREFIX_KEY_{id}";
    }
}