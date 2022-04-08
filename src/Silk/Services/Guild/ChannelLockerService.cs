using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Results;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Data.Entities.Channels;
using Silk.Data.MediatR.Channels;
using Silk.Extensions;
using Silk.Services.Data;
using Silk.Services.Interfaces;
using Silk.Shared.Types;

namespace Silk.Services.Guild;

public class ChannelLockerService : IHostedService
{
    private readonly IMediator                     _mediator;
    private readonly IChannelLoggingService        _channelLogger;
    private readonly GuildConfigCacheService       _config;
    private readonly IDiscordRestUserAPI           _users;
    private readonly IDiscordRestGuildAPI          _guilds;
    private readonly IDiscordRestChannelAPI        _channels;
    private readonly ILogger<ChannelLockerService> _logger;
    
    private readonly List<ChannelLockEntity> _locks = new();
    
    private readonly AsyncTimer _timer;
    
    public ChannelLockerService
    (
        IMediator mediator,
        IChannelLoggingService channelLogger, 
        GuildConfigCacheService config,
        IDiscordRestUserAPI users,
        IDiscordRestGuildAPI guilds,
        IDiscordRestChannelAPI channels,
        ILogger<ChannelLockerService> logger
    )
    {
        _mediator      = mediator;
        _channelLogger = channelLogger;
        _config        = config;
        _users         = users;
        _guilds        = guilds;
        _channels      = channels;
        _logger        = logger;

        _timer = new(UnlockChannels, TimeSpan.FromSeconds(5), true);
    }
    
    public async Task<Result> LockChannelAsync
    (
        Snowflake requesterID,
        Snowflake channelID,
        Snowflake guildID,
        DateTimeOffset? unlockAt = null,
        string reason = "Not Given."
    )
    {
        var channelResult = await _channels.GetChannelAsync(channelID);

        if (!channelResult.IsDefined(out var channel))
            return Result.FromError(channelResult.Error!);

        var permissionResult = await EnsureCorrectPermissionsExistAsync(channelID, guildID);
        
        if (!permissionResult.IsSuccess)
            return permissionResult;

        var topRole = await GetTopRoleAsync(guildID);
        var roleMap = await GetRoleMapAsync(guildID);
        
        if (!topRole.IsSuccess || !roleMap.IsSuccess)
            return Result.FromError(topRole.Error ?? roleMap.Error!);
        
        var overwrites = channel.PermissionOverwrites.IsDefined(out var ovr) ? ovr : Array.Empty<IPermissionOverwrite>();

        var roles = new List<Snowflake>();

        var newOverwrites = new List<IPartialPermissionOverwrite>();
        
        foreach (var overwrite in overwrites)
        {
            if (overwrite.Type is not PermissionOverwriteType.Role)
            {
                newOverwrites.Add(overwrite);
                continue;
            }

            if (roleMap.Entity[overwrite.ID] >= roleMap.Entity[topRole.Entity])
            {
                newOverwrites.Add(overwrite);
                continue;
            }
            
            roles.Add(overwrite.ID);

            newOverwrites.Add(new PartialPermissionOverwrite(overwrite.ID, default, default, new DiscordPermissionSet(DiscordPermission.SendMessages)));
        }
        
        var lockResult = await _channels.ModifyChannelAsync(channelID, permissionOverwrites: newOverwrites);

        if (!lockResult.IsSuccess)
        {
            _logger.LogWarning("Failed to lock channel in guild {GuildID}.", guildID);
            return Result.FromError(lockResult.Error!);
        }
        
        var cLock = await _mediator.Send(new LockChannel.Request(channelID, guildID, requesterID, roles, unlockAt, reason));

        _locks.RemoveAll(cl => cl.Id == cLock.Id);
        
        _locks.Add(cLock);

        var config = await _config.GetModConfigAsync(guildID);

        if (config.Logging.LogInfractions)
        {
            var logContent = $"**<@{requesterID}>** Locked <#{channelID}> {(unlockAt is null ? "indefinitely" : $"until {unlockAt.Value.ToTimestamp(TimestampFormat.LongDateTime)}")}. Reason: {reason}";

            await _channelLogger.LogAsync(config.Logging.UseWebhookLogging, config.Logging.Infractions!, logContent, null, null);
        }
        
        return Result.FromSuccess();
    }

    public async Task<Result> UnlockChannelAsync(Snowflake userID, Snowflake channelID, Snowflake guildID, string reason)
    {
        var queuedLock = _locks.FirstOrDefault(l => l.ChannelID == channelID);
        
        if (queuedLock == null)
            return Result.FromError(new NotFoundError());

        _locks.Remove(queuedLock);
        
        var result = await _mediator.Send(new UnlockChannel.Request(channelID));
        
        if (!result.IsSuccess)
            return result;

        var channelResult = await _channels.GetChannelAsync(channelID);

        if (!channelResult.IsDefined(out var channel))
            return Result.FromError(new NotFoundError());
        
        var permissionResult = await EnsureCorrectPermissionsExistAsync(channelID, queuedLock.GuildID);

        if (!permissionResult.IsSuccess)
        {
            _logger.LogError("Channel unlock request submitted, but permissions do not allow it to be unlocked.");
            return permissionResult;
        }
        
        var newOverwrites = new List<IPartialPermissionOverwrite>();
        
        var overwrites = channel.PermissionOverwrites.IsDefined(out var ovr) ? ovr : Array.Empty<IPermissionOverwrite>();

        foreach (var overwrite in overwrites)
        {
            if (overwrite.Type is not PermissionOverwriteType.Role)
            {
                newOverwrites.Add(overwrite);
                continue;
            }

            if (queuedLock.LockedRoles.Any(lr => lr == overwrite.ID))
            {
                newOverwrites.Add
                    (
                     new PartialPermissionOverwrite
                         (
                          overwrite.ID,
                            default,
                            new DiscordPermissionSet(overwrite.Allow.GetPermissions().Append(DiscordPermission.SendMessages).ToArray()),
                            default
                         )
                    );
            }
        }
        
        var unlockResult = await _channels.ModifyChannelAsync(channelID, permissionOverwrites: newOverwrites);

        if (!unlockResult.IsSuccess)
            return Result.FromError(unlockResult.Error);
        
        var config = await _config.GetModConfigAsync(guildID);

        if (config.Logging.LogInfractions)
        {
            var logContent = $"**<@{userID}>** Unlocked <#{channelID}> Reason: {reason}";

            await _channelLogger.LogAsync(config.Logging.UseWebhookLogging, config.Logging.Infractions!, logContent, null, null);
        }
        
        return Result.FromSuccess();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting channel locker...");

        var locks = await _mediator.Send(new GetChannelLocks.Request(), cancellationToken);
        
        _locks.AddRange(locks);
        
        _timer.Start();
        
        _logger.LogInformation("Channel locker started.");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
    }


    private async Task UnlockChannels()
    {
        var selfResult = await _users.GetCurrentUserAsync();

        if (!selfResult.IsDefined(out var self))
            return;
        
        foreach (var cLock in _locks.ToArray())
        {
            if (cLock.UnlocksAt <= DateTimeOffset.UtcNow)
            {
                var lockResult = await UnlockChannelAsync(self.ID, cLock.ChannelID, cLock.GuildID, "Automatic unlock; lock expired.");
                
                if (!lockResult.IsSuccess)
                    _logger.LogError("Failed to unlock channel in {GuildID}", cLock.GuildID);
            }
        }
    }

    private async Task<Result<Snowflake>> GetTopRoleAsync(Snowflake guildID)
    {
        var selfResult = await _users.GetCurrentUserAsync();
        
        if (!selfResult.IsSuccess)
            return Result<Snowflake>.FromError(selfResult.Error);

        var roles = await _guilds.GetGuildRolesAsync(guildID);
        
        if (!roles.IsSuccess)
            return Result<Snowflake>.FromError(roles.Error);
        
        var memberResult = await _guilds.GetGuildMemberAsync(guildID, selfResult.Entity.ID);
        
        if (!memberResult.IsSuccess)
            return Result<Snowflake>.FromError(memberResult.Error);

        return roles.Entity.Where(r => memberResult.Entity.Roles.Contains(r.ID)).MaxBy(r => r.Position)!.ID;
    }
    
    private async Task<Result<Dictionary<Snowflake, int>>> GetRoleMapAsync(Snowflake guildID)
    {
        var roleResult = await _guilds.GetGuildRolesAsync(guildID);
        
        if (!roleResult.IsSuccess)
            return Result<Dictionary<Snowflake, int>>.FromError(roleResult.Error);

        return Result<Dictionary<Snowflake, int>>.FromSuccess(roleResult.Entity.ToDictionary(r => r.ID, r => r.Position));
    }

    private async Task<Result> EnsureCorrectPermissionsExistAsync(Snowflake channelID, Snowflake guildID)
    {
        var rolesResult = await _guilds.GetGuildRolesAsync(guildID);
        
        if (!rolesResult.IsSuccess)
            return Result.FromError(new InvalidOperationError("Failed to fetch guild roles."));

        var channelResult = await _channels.GetChannelAsync(channelID);
        
        if (!channelResult.IsSuccess)
            return Result.FromError(new InvalidOperationError("Failed to fetch channel."));

        var overwrites = channelResult.Entity.PermissionOverwrites.IsDefined(out var ovrs) ? ovrs : Array.Empty<IPermissionOverwrite>();

        var selfResult = await _users.GetCurrentUserAsync();
        
        if (!selfResult.IsSuccess)
            return Result.FromError(new InvalidOperationError("Failed to fetch current user."));
        
        var memberResult = await _guilds.GetGuildMemberAsync(guildID, selfResult.Entity.ID);
        
        if (!memberResult.IsSuccess)
            return Result.FromError(new InvalidOperationError("Failed to fetch current user."));

        var selfRoles = rolesResult.Entity.Where(r => memberResult.Entity.Roles.Contains(r.ID)).ToArray();

        var permissions = DiscordPermissionSet.ComputePermissions(selfResult.Entity.ID, rolesResult.Entity.Single(r => r.ID == guildID), selfRoles, overwrites);
        
        if (!permissions.HasPermission(DiscordPermission.ManageChannels))
            return Result.FromError(new PermissionDeniedError("I don't have permission to the channel!"));
        
        return Result.FromSuccess();
    }
}