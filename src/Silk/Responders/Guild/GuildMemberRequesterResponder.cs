using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OneOf.Types;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Gateway.Commands;
using Remora.Discord.Caching;
using Remora.Discord.Caching.Abstractions.Services;
using Remora.Discord.Caching.Services;
using Remora.Discord.Gateway;
using Remora.Discord.Gateway.Responders;
using Remora.Rest;
using Remora.Results;
using Silk.Extensions.Remora;
using Silk.Services.Data;

namespace Silk.Responders;

public class GuildMemberRequesterResponder : IResponder<IGuildCreate>//, IResponder<IGatewayEvent>

{
    private static readonly SemaphoreSlim _sync         = new(1);
    private static readonly TimeSpan      _minimumDelta = TimeSpan.FromMinutes(1);
    
    private readonly CacheService                           _cache;
    private readonly ICacheProvider                         _provider;
    private readonly IRestHttpClient                        _client;
    private readonly GuildCacherService                     _memberCacher;
    private readonly ILogger<GuildMemberRequesterResponder> _logger;

    public GuildMemberRequesterResponder
    (
        CacheService                           cache,
        ICacheProvider                         provider,
        IRestHttpClient                        client,
        GuildCacherService                     memberCacher,
        ILogger<GuildMemberRequesterResponder> logger
    )
    {
        _cache        = cache;
        _provider     = provider;
        _client       = client;
        _memberCacher = memberCacher;
        _logger       = logger;
    }
    
    public async Task<Result> RespondAsync(IGuildCreate gatewayEvent, CancellationToken ct = default)
    {
        if (gatewayEvent.IsUnavailable.IsDefined(out var unavailable) && unavailable)
            return Result.FromSuccess(); // Thanks, Night.
        
        await _sync.WaitAsync(ct);

        var attempts = 1;
        var chunks   = Math.Max(1, gatewayEvent.MemberCount.Value / 1000);

        var backoff = _minimumDelta * (chunks % 5);

        while (true)
        {
            var memberResult = await _client.GetGuildMembersAsync(_provider, gatewayEvent.ID);

            if (memberResult.IsDefined(out var members))
            {
                await _cache.CacheAsync(KeyHelpers.CreateGuildMembersKey(gatewayEvent.ID, default, default), members, ct);
                await _memberCacher.CacheMembersAsync(gatewayEvent.ID, members);
                
                break;
            }
            else
            {
                if (attempts++ < 3)
                {
                    _logger.LogError("Failed to query members for {Guild} after 3 attempts. Giving up.", gatewayEvent.ID);

                    return Result.FromError(new NotFoundError($"Failed to retreive guild members for {gatewayEvent.ID}."));
                }

                var expBackoff = backoff / 2 * attempts;

                _logger.LogError("Failed to fetch guild members for {Guild}, trying again in {Attempt:hh:mm:ss}", gatewayEvent.ID, expBackoff);

                await Task.Delay(expBackoff, ct);
            }
        }

        await Task.Delay(backoff / attempts, ct);
        
        _sync.Release();
        
        return Result.FromSuccess();
    }
}