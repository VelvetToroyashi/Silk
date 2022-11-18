using System;
using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Microsoft.Extensions.Caching.Memory;
using Remora.Rest.Core;
using Silk.Data.Entities;
using Silk.Data.MediatR.Guilds;
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
        var guildCacheKey = SilkKeyHelper.GenerateGuildKey(guildId);
        _cache.Remove(guildCacheKey);
    }

    public virtual async ValueTask<GuildConfigEntity> GetConfigAsync(Snowflake guildId)
    {
        var cacheKey = SilkKeyHelper.GenerateGuildKey(guildId);
        
        if (_cache.TryGetValue(cacheKey, out GuildConfigEntity config))
            return config;
        
        return await GetConfigFromDatabaseAsync(guildId);
    }

    private async Task<GuildConfigEntity> GetConfigFromDatabaseAsync(Snowflake guildId)
    {
        var configuration = await _mediator.Send(new GetOrCreateGuildConfig.Request(guildId, StringConstants.DefaultCommandPrefix), CancellationToken.None);
        var cacheKey = SilkKeyHelper.GenerateGuildKey(guildId);
        
        _cache.Set(cacheKey, configuration, _defaultCacheExpiration);
        return configuration;
    }
}