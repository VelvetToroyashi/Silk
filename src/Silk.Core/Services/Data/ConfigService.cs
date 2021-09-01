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

		public ConfigService(IMemoryCache cache, IMediator mediator, ICacheUpdaterService updater)
		{
			_cache = cache;
			_mediator = mediator;
			updater.ConfigUpdated += u =>
			{
				cache.Remove(u);
				cache.Remove(u + "_mod");
			};
		}

		public async ValueTask<GuildConfigEntity> GetConfigAsync(ulong guildId)
		{
			if (_cache.TryGetValue(guildId, out GuildConfigEntity config)) return config;
			return await GetConfigFromDatabaseAsync(guildId);
		}

		public async ValueTask<GuildModConfigEntity> GetModConfigAsync(ulong guildId)
		{
			if (_cache.TryGetValue($"{guildId}_mod", out GuildModConfigEntity config)) return config;
			return await GetModConfigFromDatabaseAsync(guildId);
		}
		private async Task<GuildModConfigEntity> GetModConfigFromDatabaseAsync(ulong guildId)
		{
			GuildModConfigEntity? configuration = await _mediator.Send(new GetGuildModConfigRequest(guildId), CancellationToken.None);
			_cache.Set($"{guildId}_mod", configuration, TimeSpan.FromHours(1));
			return configuration;
		}

		private async Task<GuildConfigEntity> GetConfigFromDatabaseAsync(ulong guildId)
		{
			GuildConfigEntity configuration = await _mediator.Send(new GetGuildConfigRequest(guildId), CancellationToken.None);
			_cache.Set(guildId, configuration, TimeSpan.FromHours(1));
			return configuration;
		}
	}
}