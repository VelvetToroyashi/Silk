using System.Text.Json;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Distributed;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Caching.Services;

namespace Silk.Remora.RedisCache;

[PublicAPI]
public class RedisCacheService 
{
    private readonly IDistributedCache     _cache;
    private readonly CacheSettings         _settings;
    private readonly JsonSerializerOptions _jsonOptions;
    
    public RedisCacheService(IDistributedCache cache, CacheSettings settings, JsonSerializerOptions jsonOptions)
    {
        _cache = cache;
        _settings = settings;
        _jsonOptions = jsonOptions;
    }

    public async Task<T?> EvictAsync<T>(object key) => default;

    public async Task<T?> TryGetValueAsync<T>(object key)
    {
        var cacheKey = TryDeconstructKey(key);
    
        var cachedValue = await _cache.GetAsync(cacheKey);
        
        if (cachedValue is null)
            return default;

        await _cache.RefreshAsync(cacheKey);
        
        return JsonSerializer.Deserialize<T>(cachedValue, _jsonOptions);
    }

    public async Task CacheAsync<T>(object key, T instance) where T : class
    {
        Task cacheTask = instance switch
        {
            IBan            ban         => CacheBanAsync(key, ban),
            IChannel        channel     => CacheChannelAsync(key, channel),
            IEmoji          emoji       => CacheEmojiAsync(key, emoji),
            IGuild          guild       => CacheGuildAsync(key, guild),
            IGuildMember    member      => CacheGuildMemberAsync(key, member),
            IGuildPreview   preview     => CacheGuildPreviewAsync(key, preview),
            IIntegration    integration => CacheIntegrationAsync(key, integration),
            IInvite         invite      => CacheInviteAsync(key, invite),
            IMessage        message     => CacheMessageAsync(key, message),
            ITemplate       template    => CacheTemplateAsync(key, template),
            IWebhook        webhook     => CacheWebhookAsync(key, webhook),
            _                           => CacheInstanceAsync(key, instance)
        };

        await cacheTask;
    }
    
    
    private async Task CacheBanAsync(object key, IBan ban) { }
    
    private async Task CacheChannelAsync(object key, IChannel channel) { }
    
    private async Task CacheEmojiAsync(object key, IEmoji emoji) { }
    
    private async Task CacheGuildAsync(object key, IGuild guild) { }
    
    private async Task CacheGuildMemberAsync(object key, IGuildMember member) { }
    
    private async Task CacheGuildPreviewAsync(object key, IGuildPreview preview) { }
    
    private async Task CacheIntegrationAsync(object key, IIntegration integration) { }
    
    private async Task CacheInviteAsync(object key, IInvite invite) { }
    
    private async Task CacheMessageAsync(object key, IMessage message) { }
    
    private async Task CacheTemplateAsync(object key, ITemplate template) { }



    private async Task CacheWebhookAsync(object key, IWebhook webhook) { }

    private async Task CacheInstanceAsync<T>(object key, T instance) where T : class { }

    private TimeSpan? GetAbsoluteExpirationFor<T>() => _settings.GetAbsoluteExpirationOrDefault<T>();
    private TimeSpan? GetSlidingExpirationFor<T>()  => _settings.GetSlidingExpirationOrDefault<T>();

    private string TryDeconstructKey(object key)
    {
        var keyType = key.GetType();

        if (!keyType.IsGenericType || keyType.GetGenericTypeDefinition() != typeof(ValueTuple<,>))
            return key.ToString()!;
        
        var keyTypeType = keyType.GetGenericArguments()[0];
        var keyTypeValue = keyType.GetGenericArguments()[1];
        
        var keyTypeTypeName = keyTypeType.Name;
        
        return $"{keyTypeTypeName}:{keyTypeValue}";
    }
    
}