using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Data.Entities;
using Silk.Data.MediatR.Guilds;
using Silk.Data.MediatR.Users;
using Silk.Shared.Constants;
using Silk.Shared.Types;

namespace Silk.Services.Data;

public class GuildCacherService
{
    /// <summary>
    /// The time in which a guild is considered new, if the joined timestamp is within this threshold.
    /// </summary>
    // If we joined within the last 30 seconds, we consider it new.
    // This is bound to change in the future. For now 30s is good enough to allow for caching,
    // as well as accommodating for any responder delays.
    private readonly TimeSpan _joinedTimestampThreshold = 30.Seconds();

    private readonly object _obj = new();

    private const string GuildJoinThankYouMessage = "Hiya! My name is Silk! I hope to satisfy your entertainment and moderation needs.\n\n"      +
                                                    $"I respond to mentions and `{StringConstants.DefaultCommandPrefix}` by default, "           +
                                                    $"but you can change that with `{StringConstants.DefaultCommandPrefix}prefix`\n\n"           +
                                                    "There's also a variety of :sparkles: slash commands :sparkles: if those suit your fancy!\n" +
                                                    "(Currently unavailable in V3, sorry! :c)";
    private readonly IDiscordRestChannelAPI _channelApi;
    private readonly IDiscordRestGuildAPI   _guildApi;

    private readonly IMemoryCache _cache;
    
    private readonly ILogger<GuildCacherService> _logger;

    private readonly IMediator _mediator;

    private readonly IEmbed _onGuildJoinEmbed = new Embed(
                                                          Title: "Thank you for adding me!",
                                                          Description: GuildJoinThankYouMessage,
                                                          Colour: Color.CornflowerBlue);
    private readonly IDiscordRestUserAPI _userApi;

    /// <summary>
    ///     Required permissions to send a welcome message to a new guild.
    /// </summary>
    private readonly DiscordPermission _welcomeMessagePermissions = (DiscordPermission)((int)DiscordPermission.SendMessages | (int)DiscordPermission.EmbedLinks);

    public GuildCacherService
    (
        IMediator                   mediator,
        IDiscordRestUserAPI         userApi,
        IDiscordRestGuildAPI        guildApi,
        IDiscordRestChannelAPI      channelApi,
        ILogger<GuildCacherService> logger,
        IMemoryCache cache
    )
    {
        _mediator   = mediator;
        _userApi    = userApi;
        _guildApi   = guildApi;
        _channelApi = channelApi;
        _logger     = logger;
        _cache = cache;
    }

    public async Task<Result> GreetGuildAsync(IGuild guild)
    {
        //It's worth noting that there's a chance that one could pass
        //a guild fetched from REST here, which typically doesn't have
        //channels defined, which is a big issue, but that's on the caller

        var currentUserResult = await _userApi.GetCurrentUserAsync();

        if (!currentUserResult.IsSuccess)
            return Result.FromError(currentUserResult.Error);

        var currentMemberResult = await _guildApi.GetGuildMemberAsync(guild.ID, currentUserResult.Entity.ID);

        if (!currentMemberResult.IsSuccess)
            return Result.FromError(currentMemberResult.Error);

        var currentUser   = currentUserResult.Entity;
        
        var currentMember = currentMemberResult.Entity;

        // check if we joined within the last 30 seconds
        if (currentMember.JoinedAt + _joinedTimestampThreshold < DateTimeOffset.Now)
            return Result.FromSuccess();
        
        var channels = guild.Channels.Value;

        IChannel? availableChannel = null;
        
        _logger.LogTrace("Attempting to find open channel to send welcome message to");
        foreach (var channel in channels)
        {
            if (channel.Type is not ChannelType.GuildText)
                continue;

            var overwrites = channel.PermissionOverwrites.Value;

            var permissions = DiscordPermissionSet.ComputePermissions
                (
                 currentUser.ID,
                 guild.Roles.Single(r => r.ID == guild.ID),
                 guild.Roles.Where(r => currentMember.Roles.Contains(r.ID)).ToArray(),
                 overwrites
                );

            if (permissions.HasPermission(_welcomeMessagePermissions))
            {
                availableChannel = channel;
                
                _logger.LogDebug("Found available message channel");
                break;
            }
        }

        if (availableChannel is null)
        {
            _logger.LogWarning("Unable to find open channel to send thank you message to in guild {GuildID}", guild.ID);
        }
        else
        {
            var send = await _channelApi.CreateMessageAsync(availableChannel.ID, embeds: new[] { _onGuildJoinEmbed });

            if (send.IsSuccess)
            {
                _logger.LogInformation("Sent thank you message to new guild: {GuildID}", guild.ID);
                return Result.FromSuccess();
            }
        }

        return Result.FromSuccess();
    }
    

    public async Task<Result> CacheGuildAsync(Snowflake guildID, IReadOnlyList<IGuildMember> members)
    {
        await _mediator.Send(new GetOrCreateGuild.Request(guildID, StringConstants.DefaultCommandPrefix));

        return await CacheMembersAsync(guildID, members);
    }
    
    private async Task<Result> CacheMembersAsync(Snowflake guildID, IReadOnlyList<IGuildMember> members)
    {
        var users = members.Where(u => u.User.IsDefined())
                           .Select(u => (u.User.Value.ID, u.JoinedAt))
                           .Select((uj, _) =>
                            {
                                (var id, var joinedAt) = uj;
                                return new UserEntity
                                {
                                    ID      = id,
                                    GuildID = guildID,
                                    History = new() { JoinDates = new() {joinedAt} }
                                };
                            });

        await _mediator.Send(new BulkAddUser.Request(users));

        LogAndCacheGuild(guildID, members);

        return Result.FromSuccess();
    }

    
    private void LogAndCacheGuild(Snowflake guildID, IReadOnlyList<IGuildMember> members)
    {
        lock (_obj)
        {
            var currentGuildCount   = _cache.Get<int>(SilkKeyHelper.GenerateGuildCountKey());
            var currentGuildCounter = _cache.GetOrCreate(SilkKeyHelper.GenerateCurrentGuildCounterKey(), _ => 1);

            _logger.LogInformation("Received guild [{CurrentGuild,2}/{GuildCount,-2}]", currentGuildCounter, currentGuildCount);

            _cache.Set(SilkKeyHelper.GenerateGuildIdentifierKey(guildID), true);
            _cache.Set(SilkKeyHelper.GenerateGuildMemberCountKey(guildID), members.Count);
            _cache.Set(SilkKeyHelper.GenerateCurrentGuildCounterKey(), currentGuildCounter + 1);
        }
    }
}