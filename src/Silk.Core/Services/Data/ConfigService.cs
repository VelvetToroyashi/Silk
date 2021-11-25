using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Silk.Core.Data.Entities;
using Silk.Core.Data.MediatR.Guilds;
using Silk.Core.Data.MediatR.Guilds.Config;
using Silk.Core.Services.Interfaces;

namespace Silk.Core.Services.Data
{
	public class ConfigService
	{
		private readonly IMemoryCache _cache;
		private readonly IMediator _mediator;
		private readonly TimeSpan _defaultCacheExpiration = TimeSpan.FromMinutes(60);

		public ConfigService(IMemoryCache cache, IMediator mediator, ICacheUpdaterService updater)
		{
			_cache = cache;
			_mediator = mediator;
			updater.ConfigUpdated += OnConfigUpdated;
		}

		private void OnConfigUpdated(ulong guildId)
		{
			var guildCacheKey = GetCacheGuildConfigKey(guildId);
			var guildModCacheKey = GetCacheGuildModConfigKey(guildId);
			_cache.Remove(guildCacheKey);
			_cache.Remove(guildModCacheKey);
		}

		private object GetCacheGuildConfigKey(ulong guildId) => guildId;
		private object GetCacheGuildModConfigKey(ulong guildId) => $"{guildId}_mod";

		public async ValueTask<GuildConfigEntity> GetConfigAsync(ulong guildId)
		{
			var cacheKey = GetCacheGuildConfigKey(guildId);
			if (_cache.TryGetValue(cacheKey, out GuildConfigEntity config)) return config;
			return await GetConfigFromDatabaseAsync(guildId);
		}

		public async ValueTask<GuildModConfigEntity> GetModConfigAsync(ulong guildId)
		{
			var cacheKey = GetCacheGuildModConfigKey(guildId);
			if (_cache.TryGetValue(cacheKey, out GuildModConfigEntity config)) return config;
			return await GetModConfigFromDatabaseAsync(guildId);
		}
		private async Task<GuildModConfigEntity> GetModConfigFromDatabaseAsync(ulong guildId)
		{
			GuildModConfigEntity? configuration = await _mediator.Send(new GetGuildModConfigRequest(guildId), CancellationToken.None);
			var cacheKey = GetCacheGuildModConfigKey(guildId);
			_cache.Set(cacheKey, configuration, _defaultCacheExpiration);
			return configuration;
		}

		private async Task<GuildConfigEntity> GetConfigFromDatabaseAsync(ulong guildId)
		{
			GuildConfigEntity? configuration = await _mediator.Send(new GetGuildConfigRequest(guildId), CancellationToken.None);
			var cacheKey = GetCacheGuildConfigKey(guildId);
			_cache.Set(cacheKey, configuration, _defaultCacheExpiration);
			return configuration;
		}
	}
}