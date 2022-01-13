using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Remora.Rest.Core;
using Silk.Data.Entities;
using Silk.Data.MediatR.Guilds;
using Silk.Data.MediatR.Guilds.Config;
using Silk.Shared.Constants;
using Silk.Shared.Types;

namespace Silk.Services.Data;

public class GuildConfigCacheService
{
    private readonly IMemoryCache _cache;
    private readonly IMediator    _mediator;
    private readonly TimeSpan     _defaultCacheExpiration = TimeSpan.FromMinutes(10);

    public GuildConfigCacheService(IMemoryCache cache, IMediator mediator)
    {
        _cache    = cache;
        _mediator = mediator;
    }

    public void PurgeCache(Snowflake guildId)
    {
        object? guildCacheKey    = ConfigKeyHelper.GenerateGuildKey(guildId);
        object? guildModCacheKey = ConfigKeyHelper.GenerateGuildModKey(guildId);

        _cache.Remove(guildCacheKey);
        _cache.Remove(guildModCacheKey);
    }

    public async ValueTask<GuildConfigEntity> GetConfigAsync(Snowflake guildId)
    {
        object cacheKey = ConfigKeyHelper.GenerateGuildKey(guildId);
        return _cache.TryGetValue(cacheKey, out GuildConfigEntity config)
            ? config
            : await GetConfigFromDatabaseAsync(guildId);
    }

    public async ValueTask<GuildModConfigEntity> GetModConfigAsync(Snowflake guildId)
    {
        object cacheKey = ConfigKeyHelper.GenerateGuildModKey(guildId);
        return _cache.TryGetValue(cacheKey, out GuildModConfigEntity config) ? config : await GetModConfigFromDatabaseAsync(guildId);
    }


    private async Task<GuildModConfigEntity> GetModConfigFromDatabaseAsync(Snowflake guildId)
    {
        GuildModConfigEntity? configuration = await _mediator.Send(new GetOrCreateGuildModConfig.Request(guildId, StringConstants.DefaultCommandPrefix), CancellationToken.None);
        object                cacheKey      = ConfigKeyHelper.GenerateGuildModKey(guildId);
        _cache.Set(cacheKey, configuration, _defaultCacheExpiration);
        return configuration;
    }

    private async Task<GuildConfigEntity> GetConfigFromDatabaseAsync(Snowflake guildId)
    {
        GuildConfigEntity configuration = await _mediator.Send(new GetOrCreateGuildConfig.Request(guildId, StringConstants.DefaultCommandPrefix), CancellationToken.None);
        object?           cacheKey      = ConfigKeyHelper.GenerateGuildKey(guildId);
        _cache.Set(cacheKey, configuration, _defaultCacheExpiration);
        return configuration;
    }
}