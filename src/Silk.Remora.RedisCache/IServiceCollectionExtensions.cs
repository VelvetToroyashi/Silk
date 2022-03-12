using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Remora.Discord.Caching.Services;

namespace Silk.Remora.RedisCache;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddRedisCaching(this IServiceCollection services, Action<RedisCacheOptions>? configureAction = null)
    {
        configureAction ??= _ => { };
        services.AddStackExchangeRedisCache(configureAction);
        
        services.AddOptions<CacheSettings>();
        services.AddSingleton<RedisCacheService>();

        return services;
    }
}