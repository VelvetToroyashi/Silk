using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Silk.Core.Data.MediatR.Unified.Guilds;
using Silk.Core.Data.Models;

namespace Silk.Core.Discord.Services
{
    public class ConfigService
    {
        private readonly IMemoryCache _cache;
        private readonly IMediator _mediator;
        public ConfigService(IMemoryCache cache, IMediator mediator)
        {
            _cache = cache;
            _mediator = mediator;
        }

        public async ValueTask<GuildConfig> GetConfigAsync(ulong guildId)
        {
            if (_cache.TryGetValue(guildId, out GuildConfig config)) return config;
            return await GetConfigFromDatabaseAsync(guildId);
        }

        private async Task<GuildConfig> GetConfigFromDatabaseAsync(ulong guildId)
        {
            GuildConfig configuration = await _mediator.Send(new GetGuildConfigRequest(guildId), CancellationToken.None);
            _cache.Set(guildId, configuration, TimeSpan.FromHours(1));
            return configuration;
        }
    }
}