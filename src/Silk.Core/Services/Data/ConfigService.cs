using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Silk.Core.Data.MediatR.Guilds;
using Silk.Core.Data.MediatR.Guilds.Config;
using Silk.Core.Data.Models;
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
			updater.ConfigUpdated += u => cache.Remove(u);
		}

		public async ValueTask<GuildConfig> GetConfigAsync(ulong guildId)
		{
			if (_cache.TryGetValue(guildId, out GuildConfig config)) return config;
			return await GetConfigFromDatabaseAsync(guildId);
		}

		public async ValueTask<GuildModConfig> GetModConfigAsync(ulong guildId)
		{
			if (_cache.TryGetValue($"{guildId}_mod", out GuildModConfig config)) return config;
			return await GetModConfigFromDatabaseAsync(guildId);
		}
		private async Task<GuildModConfig> GetModConfigFromDatabaseAsync(ulong guildId)
		{
			GuildModConfig? configuration = await _mediator.Send(new GetGuildModConfigRequest(guildId), CancellationToken.None);
			_cache.Set($"{guildId}_mod", configuration, TimeSpan.FromHours(1));
			return configuration;
		}

		private async Task<GuildConfig> GetConfigFromDatabaseAsync(ulong guildId)
		{
			GuildConfig configuration = await _mediator.Send(new GetGuildConfigRequest(guildId), CancellationToken.None);
			_cache.Set(guildId, configuration, TimeSpan.FromHours(1));
			return configuration;
		}
	}
}