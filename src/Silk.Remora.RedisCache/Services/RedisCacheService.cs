using System.Reflection;
using System.Text;
using System.Text.Json;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Caching;
using Remora.Discord.Caching.Services;

namespace Silk.Remora.RedisCache;

[PublicAPI]
public class RedisCacheService 
{
    private readonly IDistributedCache     _cache;
    private readonly CacheSettings         _settings;
    private readonly JsonSerializerOptions _jsonOptions;
    
    public RedisCacheService(IDistributedCache cache, IOptions<CacheSettings> settings, IOptionsMonitor<JsonSerializerOptions> jsonOptions)
    {
        _cache = cache;
        _settings = settings.Value;
        _jsonOptions = jsonOptions.Get("Discord");
    }

    public async Task EvictAsync<T>(string key)
    {
        var settings = GetCacheOptionsFor<T>();
        
        var value = await _cache.GetAsync(key);

        if (value == null)
            return;
        
        await _cache.RemoveAsync(key);
        
        key = $"Evicted:{key}";

        await _cache.SetAsync(key, value, settings);
    }
    
    public Task<T?> TryGetEvictedValueAsync<T>(string key) => TryGetValueAsync<T>($"Evicted:{key}");
    

    public async Task<T?> TryGetValueAsync<T>(string key)
    {
        var cachedValue = await _cache.GetAsync(key);
        
        if (cachedValue is null)
            return default;

        await _cache.RefreshAsync(key);
        
        return JsonSerializer.Deserialize<T>(cachedValue, _jsonOptions);
    }

    public async Task CacheAsync<T>(string key, T instance) where T : class
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


    private async Task CacheBanAsync(string key, IBan ban)
    {
        await CacheInstanceAsync(key, ban);

        key = RedisKeyHelper.CreateUserCacheKey(ban.User.ID);
        
        await CacheAsync(key, ban.User);
    }

    private async Task CacheChannelAsync(string key, IChannel channel)
    {
        await CacheInstanceAsync(key, channel);

        if (!channel.Recipients.IsDefined(out var recipients))
            return;

        await Task.WhenAll(recipients.Select(r => CacheAsync(RedisKeyHelper.CreateUserCacheKey(r.ID), r)));
    }

    private async Task CacheEmojiAsync(string key, IEmoji emoji)
    {
        await CacheInstanceAsync(key, emoji);

        if (!emoji.User.IsDefined(out var user))
            return;
        
        key = RedisKeyHelper.CreateUserCacheKey(user.ID);
        
        await CacheAsync(key, user);
    }

    private async Task CacheGuildAsync(string key, IGuild guild)
    {
        await CacheInstanceAsync(key, guild);

        if (guild.Channels.IsDefined(out var channels))
        {
            foreach (var channel in channels)
            {
                key = RedisKeyHelper.CreateChannelCacheKey(channel.ID);

                if (channel.GuildID.HasValue || channel.Type is ChannelType.DM or ChannelType.GroupDM)
                {
                    await CacheAsync(key, channel);
                }
                else
                {
                    if (channel is Channel record)
                        await CacheAsync(key, record with { GuildID = guild.ID });
                }
            }
        }
        
        foreach (var emoji in guild.Emojis.Where(e => e.ID is not null))
        {
            key = RedisKeyHelper.CreateEmojiCacheKey(guild.ID, emoji.ID.Value);
            await CacheAsync(key, emoji);
        }

        if (guild.Members.IsDefined(out var members))
        {
            key = RedisKeyHelper.CreateGuildMembersCacheKey(guild.ID);
            await CacheAsync(key, members);

            foreach (var member in members)
            {
                if (!member.User.IsDefined(out var user))
                    continue;
                
                key = RedisKeyHelper.CreateUserCacheKey(user.ID);
                await CacheAsync(key, user);
                
                key = RedisKeyHelper.CreateGuildMemberCacheKey(guild.ID, user.ID);
                
                await CacheAsync(key, member);
            }
        }
        
        
        key = RedisKeyHelper.CreateGuildRolesCacheKey(guild.ID);
        await CacheAsync(key, guild.Roles);
           
        foreach (var role in guild.Roles)
        {
            key = RedisKeyHelper.CreateGuildRoleCacheKey(guild.ID, role.ID);
            await CacheAsync(key, role);
        }
    }

    private async Task CacheGuildMemberAsync(string key, IGuildMember member)
    {
        await CacheInstanceAsync(key, member);

        if (!member.User.IsDefined(out var user))
            return;

        key = RedisKeyHelper.CreateUserCacheKey(user.ID);

        await CacheAsync(key, user);
    }

    private async Task CacheGuildPreviewAsync(string key, IGuildPreview preview)
    {
        await CacheInstanceAsync(key, preview);

        await Task.WhenAll
            (
             preview.Emojis
                    .Where(e => e.ID is not null)
                    .Select(e => CacheAsync(RedisKeyHelper.CreateEmojiCacheKey(preview.ID, e.ID.Value), e))
            );
    }

    private async Task CacheIntegrationAsync(string key, IIntegration integration)
    {
        await CacheInstanceAsync(key, integration);

        if (!integration.User.IsDefined(out var user))
            return;

        key = RedisKeyHelper.CreateUserCacheKey(user.ID);

        await CacheAsync(key, user);
    }

    private async Task CacheInviteAsync(string key, IInvite invite)
    {
        await CacheInstanceAsync(key, invite);

        if (!invite.Inviter.IsDefined(out var inviter))
            return;

        key = RedisKeyHelper.CreateUserCacheKey(inviter.ID);
        
        await CacheAsync(key, inviter);
    }

    private async Task CacheMessageAsync(string key, IMessage message)
    {
        await CacheInstanceAsync(key, message);
        
        key = RedisKeyHelper.CreateUserCacheKey(message.Author.ID);
        
        await CacheInstanceAsync(key, message.Author);

        if (!message.ReferencedMessage.IsDefined(out var referencedMessage))
            return;
        
        key = RedisKeyHelper.CreateMessageCacheKey(referencedMessage.ChannelID, referencedMessage.ID);

        await CacheAsync(key, referencedMessage);
    }

    private async Task CacheTemplateAsync(string key, ITemplate template)
    {
        await CacheInstanceAsync(key, template);
        
        key = RedisKeyHelper.CreateUserCacheKey(template.CreatorID);

        await CacheAsync(key, template.Creator);
    }
    
    private async Task CacheWebhookAsync(string key, IWebhook webhook)
    {
        await CacheInstanceAsync(key, webhook);

        if (!webhook.User.IsDefined(out var user))
            return;

        key = RedisKeyHelper.CreateUserCacheKey(user.ID);
        
        await CacheAsync(key, user);
    }

    private async Task CacheInstanceAsync<T>(string key, T instance) where T : class
    {
        var cacheSettings = GetCacheOptionsFor<T>();

        await EvictAsync<T>(key);
        
        var serializedValue = JsonSerializer.SerializeToUtf8Bytes(instance, _jsonOptions);

        await _cache.SetAsync(key, serializedValue, cacheSettings);
    }

    private DistributedCacheEntryOptions GetCacheOptionsFor<T>()
    {
        var absolute = GetAbsoluteExpirationFor<T>();
        var sliding = GetSlidingExpirationFor<T>();

        var options = new DistributedCacheEntryOptions();

        if (absolute is not null)
            options.SetAbsoluteExpiration(absolute.Value);

        if (sliding is not null && absolute is null)
            options.SetSlidingExpiration(sliding.Value);
        
        return options;
    }
    
    private TimeSpan? GetAbsoluteExpirationFor<T>() => _settings.GetAbsoluteExpirationOrDefault<T>();
    private TimeSpan? GetSlidingExpirationFor<T>()  => _settings.GetSlidingExpirationOrDefault<T>();
}