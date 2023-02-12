using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using Mediator;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Gateway.Commands;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Abstractions.Results;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Results;
using Remora.Rest.Core;
using Remora.Rest.Results;
using Remora.Results;
using Silk.Data.DTOs.Guilds;
using Silk.Data.Entities;
using Silk.Data.MediatR.Guilds;
using Silk.Data.MediatR.Infractions;
using Silk.Errors;
using Silk.Extensions;
using Silk.Extensions.Remora;
using Silk.Services.Data;
using Silk.Services.Interfaces;
using Silk.Shared.Constants;
using Silk.Shared.Types;
using Silk.Utilities;

namespace Silk.Services.Guild;

public sealed class InfractionService : IHostedService, IInfractionService
{
    private static readonly TimeSpan _maxTimeoutDuration = TimeSpan.FromDays(28);
    
    private const string SilkWebhookName = "Silk! Logging";
    private const string SemiSuccessfulAction = "The action completed, but there was an error processing the infraction.";
    
    private readonly AsyncTimer _queueTimer;
    
    private readonly IMediator                  _mediator;
    private readonly IShardIdentification       _shard;
    private readonly IDiscordRestUserAPI        _users;
    private readonly IDiscordRestGuildAPI       _guilds;
    private readonly IDiscordRestChannelAPI     _channels;
    private readonly IDiscordRestWebhookAPI     _webhooks;
    private readonly IChannelLoggingService     _channelLogger;
    private readonly ILogger<InfractionService> _logger;

    private readonly List<Infraction> _queue = new();
    public InfractionService
    (
        IMediator                  mediator,
        IShardIdentification       shard,
        IDiscordRestUserAPI        users,
        IDiscordRestGuildAPI       guilds,
        IDiscordRestChannelAPI     channels,
        IDiscordRestWebhookAPI     webhooks, 
        IChannelLoggingService     channelLogger,
        ILogger<InfractionService> logger
    )
    {
        _mediator      = mediator;
        _shard         = shard;
        _users         = users;
        _guilds        = guilds;
        _channels      = channels;
        _webhooks      = webhooks;
        _channelLogger = channelLogger;
        _logger        = logger;

        _queueTimer = new(ProcessQueueAsync, TimeSpan.FromSeconds(5));
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting infraction service...");

        await LoadActiveInfractionsAsync();

        _queueTimer.Start();

        _logger.LogInformation("Infraction service started.");
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping infraction service...");
        _queueTimer.Stop();
        _queue.Clear();
        _logger.LogInformation("Infraction service stopped.");
    }

    private async Task ProcessQueueAsync()
    {
        for (var i = _queue.Count - 1; i >= 0; i--)
        {
            var infraction = _queue[i];

            if (infraction.Type is InfractionType.Mute or InfractionType.AutoModMute)
                await TryResetTimeoutAsync(infraction);

            if (infraction.ExpiresAt > DateTimeOffset.UtcNow)
                continue;
            
            _queue.RemoveAt(i);
            _logger.LogDebug("Removed infraction from queue.");
            
            await HandleExpiredInfractionAsync(infraction);
        }
    }

    private async Task TryResetTimeoutAsync(Infraction infraction)
    {
        var memberResult = await _guilds.GetGuildMemberAsync(infraction.GuildID, infraction.TargetID);

        if (!memberResult.IsSuccess)
            return;

        if (memberResult.Entity.CommunicationDisabledUntil.IsDefined(out var disabledUntil))
        {
            // check if disabledUntil is less than a day in the future
            if (disabledUntil.Value.AddDays(1) > DateTimeOffset.UtcNow)
                return;
            
            // if it is, either set the timeout to the expiration date, or the max expiration if it's larger
            
            var newTimeout = infraction.ExpiresAt > DateTimeOffset.UtcNow + _maxTimeoutDuration
                ? DateTimeOffset.UtcNow + _maxTimeoutDuration
                : infraction.ExpiresAt ?? DateTimeOffset.UtcNow + _maxTimeoutDuration;

            var timoutResult = await _guilds.ModifyGuildMemberAsync(infraction.GuildID, infraction.TargetID, communicationDisabledUntil: newTimeout);

            if (!timoutResult.IsSuccess) 
                _logger.LogError("Failed to reset timeout for {TargetID} in {GuildID}", infraction.TargetID, infraction.GuildID);
        }
    }

    private async Task HandleExpiredInfractionAsync(Infraction infraction)
    {
        if (infraction.Type is InfractionType.SoftBan)
        {
            var unbanResult = await UnBanAsync(infraction.GuildID, infraction.TargetID, infraction.EnforcerID, "Infraction expired.");

            if (!unbanResult.IsSuccess)
                _logger.LogError("Failed to unban user {UserID} ({GuildID}).", infraction.TargetID, infraction.GuildID);
        }
        else if (infraction.Type is InfractionType.Mute or InfractionType.AutoModMute)
        {
            var unmuteResult = await UnMuteAsync(infraction.GuildID, infraction.TargetID, infraction.EnforcerID, "Infraction expired.");
            
            if (!unmuteResult.IsSuccess)
                _logger.LogError("Failed to unmute user {UserID} ({GuildID})", infraction.TargetID, infraction.GuildID);
        }
        else
        {
            _logger.LogWarning("Detected an unknown temporary infraction type {InfractionType}.", infraction.Type);
        }
    }

    /// <inheritdoc />
    public async Task<Result<Infraction>> UpdateInfractionAsync(Infraction infraction, IUser updatedBy, string? newReason = null, Optional<TimeSpan?> newExpiration = default)
    {
        if (newExpiration.IsDefined(out var expiration) && expiration.Value < TimeSpan.Zero) 
            return Result<Infraction>.FromError(new ArgumentOutOfRangeError(nameof(newExpiration), "Expiration cannot be negative."));

        if (infraction.Type is not (InfractionType.SoftBan or InfractionType.AutoModMute or InfractionType.Mute) && newExpiration.HasValue)
            return Result<Infraction>.FromError(new ArgumentOutOfRangeError(nameof(newExpiration), "Expiration is only valid for soft bans, auto mod mutes, and mutes."));

        var infractionExpiration = newExpiration.HasValue ? DateTimeOffset.UtcNow + expiration : default(Optional<DateTimeOffset?>);
        
        var newInfraction = await _mediator.Send(new UpdateInfraction.Request(infraction.CaseID, infraction.GuildID, infractionExpiration, newReason ?? default(Optional<string>)));

        var cachedInfraction = _queue.FirstOrDefault(i => i == infraction);

        if (cachedInfraction is not null)
            _queue.Remove(cachedInfraction);
            
        _queue.Add(newInfraction);
        
        var enforcerResult = await _users.GetUserAsync(newInfraction.EnforcerID);
        var targetResult   = await _users.GetUserAsync(newInfraction.TargetID);
        
        if (!enforcerResult.IsSuccess)
            return Result<Infraction>.FromError(enforcerResult.Error);
        
        if (!targetResult.IsSuccess)
            return Result<Infraction>.FromError(targetResult.Error);
        
        var logResult = await LogInfractionUpdateAsync(newInfraction, updatedBy, targetResult.Entity, enforcerResult.Entity, DateTimeOffset.UtcNow);
        
        return logResult.IsSuccess 
            ? Result<Infraction>.FromSuccess(newInfraction)
            : Result<Infraction>.FromError(logResult.Error);
    }

    /// <inheritdoc />
    public async Task<Result<Infraction>> StrikeAsync(Snowflake guildID, Snowflake targetID, Snowflake enforcerID, string reason = "Not Given.")
    {
        using var _ = SilkMetric.InfractionDispatchTime.WithLabels("strike").NewTimer();
        
        IUser target;
        IUser enforcer;

        Result<(IUser target, IUser enforcer)> canInfractResult = await TryGetEnforcerAndTargetAsync(guildID, targetID, enforcerID);

        if (!canInfractResult.IsSuccess)
            return Result<Infraction>.FromError(canInfractResult.Error);

        (target, enforcer) = canInfractResult.Entity;

        Infraction infraction = await _mediator.Send(new CreateInfraction.Request(guildID, targetID, enforcerID, reason, InfractionType.Strike));

        var informResult = await TryInformTargetAsync(infraction, enforcer, guildID);

        if (informResult.IsSuccess && informResult.Entity)
            infraction = await _mediator.Send(new UpdateInfraction.Request(infraction.CaseID, infraction.GuildID, Notified: true));
        
        Result returnResult = await LogInfractionAsync(infraction, target, enforcer);

        return returnResult.IsSuccess
            ? Result<Infraction>.FromSuccess(infraction)
            : Result<Infraction>.FromError(new AggregateError(SemiSuccessfulAction, returnResult));
    }

    /// <inheritdoc />
    public async Task<Result<Infraction>> KickAsync(Snowflake guildID, Snowflake targetID, Snowflake enforcerID, string reason = "Not Given.", bool notify = true)
    {
        using var _ = SilkMetric.InfractionDispatchTime.WithLabels("kick").NewTimer();
        
        IUser target;
        IUser enforcer;

        Result<(IUser target, IUser enforcer)> canInfractResult = await TryGetEnforcerAndTargetAsync(guildID, targetID, enforcerID);

        if (!canInfractResult.IsSuccess)
            return Result<Infraction>.FromError(canInfractResult.Error);

        (target, enforcer) = canInfractResult.Entity;

        Infraction infraction = await _mediator.Send(new CreateInfraction.Request(guildID, targetID, enforcerID, reason, InfractionType.Kick));

        if (notify)
        {
            var memberRes = await _guilds.GetGuildMemberAsync(guildID, targetID);

            if (memberRes.IsSuccess)
            {
                var informResult = await TryInformTargetAsync(infraction, enforcer, guildID);

                if (informResult.IsDefined(out var informed) && informed)
                    infraction = await _mediator.Send(new UpdateInfraction.Request(infraction.CaseID, infraction.GuildID, Notified: true));
            }
        }
        Result kickResult = await _guilds.RemoveGuildMemberAsync(guildID, targetID, reason);

        if (!kickResult.IsSuccess)
            return Result<Infraction>.FromError(GetActionFailedErrorMessage(kickResult, "kick"));

        Result returnResult = await LogInfractionAsync(infraction, target, enforcer);

        return returnResult.IsSuccess
            ? Result<Infraction>.FromSuccess(infraction)
            : Result<Infraction>.FromError(new InfractionError(SemiSuccessfulAction, returnResult));
    }

    /// <inheritdoc />
    public async Task<Result<Infraction>> BanAsync(Snowflake guildID, Snowflake targetID, Snowflake enforcerID, int days = 0, string reason = "Not Given.", TimeSpan? expirationRelativeToNow = null, bool notify = true)
    {
        using var _ = SilkMetric.InfractionDispatchTime.WithLabels("ban").NewTimer();
        
        IUser target;
        IUser enforcer;

        Result permissionResult = await EnsureHasPermissionsAsync(guildID, enforcerID, DiscordPermission.BanMembers);

        if (!permissionResult.IsSuccess)
            return Result<Infraction>.FromError(permissionResult.Error);

        Result<(IUser target, IUser enforcer)> hierarchyResult = await TryGetEnforcerAndTargetAsync(guildID, targetID, enforcerID);

        if (!hierarchyResult.IsSuccess)
            return Result<Infraction>.FromError(hierarchyResult.Error);

        (target, enforcer) = hierarchyResult.Entity;

        Infraction infraction = await _mediator.Send(new CreateInfraction.Request(guildID, targetID, enforcerID, reason, expirationRelativeToNow.HasValue ? InfractionType.SoftBan : InfractionType.Ban));

        if (notify)
        {
            var memberRes = await _guilds.GetGuildMemberAsync(guildID, targetID);

            if (memberRes.IsSuccess)
            {
                var informResult = await TryInformTargetAsync(infraction, enforcer, guildID);

                if (informResult.IsDefined(out var informed) && informed)
                    infraction = await _mediator.Send(new UpdateInfraction.Request(infraction.CaseID, infraction.GuildID, Notified: true));
            }
        }
        
        Result banResult = await _guilds.CreateGuildBanAsync(guildID, targetID, days, reason);

        if (!banResult.IsSuccess)
            return Result<Infraction>.FromError(GetActionFailedErrorMessage(banResult, "ban"));

        Result returnResult = await LogInfractionAsync(infraction, target, enforcer);

        return returnResult.IsSuccess
            ? Result<Infraction>.FromSuccess(infraction)
            : Result<Infraction>.FromError(new InfractionError(SemiSuccessfulAction, returnResult));
    }

    /// <inheritdoc />
    public async Task<Result<Infraction>> UnBanAsync(Snowflake guildID, Snowflake targetID, Snowflake enforcerID, string reason = "Not Given.")
    {
        using var _ = SilkMetric.InfractionDispatchTime.WithLabels("unban").NewTimer();
        
        IUser target;
        IUser enforcer;

        Result banResult = await _guilds.RemoveGuildBanAsync(guildID, targetID, reason);

        if (!banResult.IsSuccess)
            return Result<Infraction>.FromError(GetActionFailedErrorMessage(banResult, "ban (required for unbanning)"));

        Infraction infraction = await _mediator.Send(new CreateInfraction.Request(guildID, targetID, enforcerID, reason, InfractionType.Unban));

        Result<(IUser target, IUser enforcer)> targetEnforcerResult = await TryGetEnforcerAndTargetAsync(guildID, targetID, enforcerID);

        if (!targetEnforcerResult.IsSuccess)
            return Result<Infraction>.FromError(targetEnforcerResult.Error);

        (target, enforcer) = targetEnforcerResult.Entity;

        Result returnResult = await LogInfractionAsync(infraction, target, enforcer);

        return returnResult.IsSuccess
            ? Result<Infraction>.FromSuccess(infraction)
            : Result<Infraction>.FromError(new InfractionError(SemiSuccessfulAction, returnResult));
    }

    /// <inheritdoc />
    public async ValueTask<bool> IsMutedAsync(Snowflake guildID, Snowflake targetID)
    {
        bool Predicate(Infraction inf) 
            => inf.Type is InfractionType.Mute 
                        or InfractionType.AutoModMute && 
               inf.GuildID == guildID && 
               inf.TargetID == targetID &&
               !inf.Processed;

        var inMemory = _queue.Any(Predicate);

        if (inMemory)
            return true;

        var inDatabase = await _mediator.Send(new GetUserInfractionsForGuild.Request(guildID, targetID));

        return inDatabase.FirstOrDefault(Predicate) is not null;
    }

    /// <inheritdoc />
    public async Task<Result<Infraction>> MuteAsync(Snowflake guildID, Snowflake targetID, Snowflake enforcerID, string reason = "Not Given.", TimeSpan? expirationRelativeToNow = null)
    {
        using var _ = SilkMetric.InfractionDispatchTime.WithLabels("mute").NewTimer();
        
        IUser target;
        IUser enforcer;
        
        Result permissionResult = await EnsureHasPermissionsAsync(guildID, enforcerID, DiscordPermission.ManageRoles);
        
        if (!permissionResult.IsSuccess)
            return Result<Infraction>.FromError(permissionResult.Error);
        
        var hierarchyResult = await TryGetEnforcerAndTargetAsync(guildID, targetID, enforcerID);
        
        if (!hierarchyResult.IsSuccess)
            return Result<Infraction>.FromError(hierarchyResult.Error);
        
        (target, enforcer) = hierarchyResult.Entity;
        
        if (await IsMutedAsync(guildID, targetID))
        {
            var userInfractions = await _mediator.Send(new GetUserInfractionsForGuild.Request(guildID, targetID));
            var muteInfraction  = userInfractions.Last(inf => inf.Type is InfractionType.AutoModMute or InfractionType.Mute && !inf.Pardoned && !inf.Processed);
            
            return await UpdateInfractionAsync(muteInfraction, enforcer, reason, expirationRelativeToNow);
        }
        
        var config = await _mediator.Send(new GetGuildConfig.Request(guildID));

        if (!config.UseNativeMute)
        {
            if (config!.MuteRoleID.Value is 0)
            {
                var muteResult = await TryCreateMuteRoleAsync(guildID);
            
                if (!muteResult.IsSuccess)
                    return Result<Infraction>.FromError(muteResult.Error);
            
                config = await _mediator.Send(new GetGuildConfig.Request(guildID));
            }

            var roleResult = await _guilds.AddGuildMemberRoleAsync(guildID, targetID, config.MuteRoleID, reason);

            if (!roleResult.IsSuccess)
                return Result<Infraction>.FromError(GetActionFailedErrorMessage(roleResult, "mute"));
        }
        else
        {
            var timeoutDuration = DateTimeOffset.UtcNow + (expirationRelativeToNow ?? _maxTimeoutDuration);
            
            var timeoutResult = await _guilds.ModifyGuildMemberAsync(guildID, targetID, communicationDisabledUntil: timeoutDuration, reason: reason);

            if (!timeoutResult.IsSuccess)
            {
                _logger.LogWarning("Failed to set timeout in {Guild}. Permission issue?", guildID);
                return Result<Infraction>.FromError(GetActionFailedErrorMessage(timeoutResult, "timeout"));
            }
        }
        
        var infractionType = enforcer.IsBot.IsDefined(out var bot) && bot ? InfractionType.AutoModMute : InfractionType.Mute;

        DateTimeOffset? infractionExpiration = expirationRelativeToNow.HasValue ? DateTimeOffset.UtcNow + expirationRelativeToNow.Value : null;

        var infraction = await _mediator.Send(new CreateInfraction.Request(guildID, targetID, enforcerID, reason, infractionType, infractionExpiration));
            
        _queue.Add(infraction);

        var informResult = await TryInformTargetAsync(infraction, enforcer, guildID);

        if (informResult.IsDefined(out var informed) && informed)
            infraction = await _mediator.Send(new UpdateInfraction.Request(infraction.CaseID, infraction.GuildID, Notified: true));
        
        var returnResult = await LogInfractionAsync(infraction, target, enforcer);
            
        return returnResult.IsSuccess
            ? Result<Infraction>.FromSuccess(infraction)
            : Result<Infraction>.FromError(new InfractionError(SemiSuccessfulAction, returnResult));

    }

    /// <inheritdoc />
    public async Task<Result<Infraction>> UnMuteAsync(Snowflake guildID, Snowflake targetID, Snowflake enforcerID, string reason = "Not Given.")
    {
        using var _ = SilkMetric.InfractionDispatchTime.WithLabels("unmute").NewTimer();
        
        IUser target;
        IUser enforcer;
        
        var canUmute = await EnsureHasPermissionsAsync(guildID, enforcerID, DiscordPermission.ManageRoles);
        
        if (!canUmute.IsSuccess)
            return Result<Infraction>.FromError(canUmute.Error);
        
        if (!await IsMutedAsync(guildID, targetID))
            return Result<Infraction>.FromError(new InvalidOperationError("That user isn't muted!"));
        
        var hierarchyResult = await TryGetEnforcerAndTargetAsync(guildID, targetID, enforcerID);
        
        if (!hierarchyResult.IsSuccess)
            return Result<Infraction>.FromError(hierarchyResult.Error);
        
        (target, enforcer) = hierarchyResult.Entity;

        var infractions = await _mediator.Send(new GetUserInfractionsForGuild.Request(guildID, targetID));

        var mute = infractions.Last(inf => inf.Type is InfractionType.Mute or InfractionType.AutoModMute && !inf.Processed);

        _queue.Remove(mute);

        await _mediator.Send(new UpdateInfraction.Request(mute.CaseID, mute.GuildID, Processed: true));
        
        var config = await _mediator.Send(new GetGuildConfig.Request(guildID));

        Result unmuteResult;

        if (!config.UseNativeMute)
            unmuteResult = await _guilds.RemoveGuildMemberRoleAsync(guildID, targetID, config.MuteRoleID, reason);
        else
            unmuteResult = await _guilds.ModifyGuildMemberAsync(guildID, targetID, communicationDisabledUntil: null);

        var infraction = await _mediator.Send(new CreateInfraction.Request(guildID, targetID, enforcerID, reason, InfractionType.Unmute));

        await LogInfractionAsync(infraction, target, enforcer);

        var informResult = await TryInformTargetAsync(infraction, enforcer, guildID);
        
        if (informResult.IsDefined(out var informed) && informed)
            infraction = await _mediator.Send(new UpdateInfraction.Request(infraction.CaseID, guildID, Notified: true));
        
        return unmuteResult.IsSuccess
            ? Result<Infraction>.FromSuccess(infraction)
            : Result<Infraction>.FromError(new InfractionError(SemiSuccessfulAction, unmuteResult));
    }

    /// <inheritdoc />
    public async Task<Result<Infraction>> AddNoteAsync(Snowflake guildID, Snowflake targetID, Snowflake enforcerID, string note)
    {
        using var     _          = SilkMetric.InfractionDispatchTime.WithLabels("note").NewTimer();
        
        Infraction infraction = await _mediator.Send(new CreateInfraction.Request(guildID, targetID, enforcerID, note, InfractionType.Note));
        
        Result<(IUser target, IUser enforcer)> hierarchyResult = await TryGetEnforcerAndTargetAsync(guildID, targetID, enforcerID);

        if (!hierarchyResult.IsSuccess)
            return Result<Infraction>.FromError(hierarchyResult.Error);

        var (target, enforcer) = hierarchyResult.Entity;
        
        var logResult = await LogInfractionAsync(infraction, target, enforcer);
        
        return logResult.IsSuccess
            ? Result<Infraction>.FromSuccess(infraction)
            : Result<Infraction>.FromError(new InfractionError(SemiSuccessfulAction, logResult));
    }

    /// <inheritdoc />
    public async Task<Result> PardonAsync(Snowflake guildID, Snowflake targetID, Snowflake enforcerID, int? caseID, string reason = "Not Given.")
    {
        using var     _      = SilkMetric.InfractionDispatchTime.WithLabels("pardon").NewTimer();
        Infraction pardon = await _mediator.Send(new CreateInfraction.Request(guildID, targetID, enforcerID, reason, InfractionType.Pardon));
        
        Result<(IUser target, IUser enforcer)> hierarchyResult = await TryGetEnforcerAndTargetAsync(guildID, targetID, enforcerID);
        
        if (!hierarchyResult.IsSuccess)
            return Result.FromError(hierarchyResult.Error);
        
        var (target, enforcer) = hierarchyResult.Entity;
        
        var infraction = await _mediator.Send(new GetUserInfractionForGuild.Request(targetID, guildID, InfractionType.Strike, caseID));
        
        if (infraction is null)
            if (caseID is null)
                return Result.FromError(new NotFoundError("That user has no infractions to be pardoned from!"));
            else
                return Result.FromError(new NotFoundError("Either that infraction doesn't exist, or it doesn't apply to this user, are you sure you have the right ID?"));
        
        await _mediator.Send(new UpdateInfraction.Request(infraction.CaseID, infraction.GuildID, AppliesToTarget: false));
        
        if (_queue.FirstOrDefault(inf => inf == infraction) is {} inf)
            _queue.Remove(inf);

        var logResult = await LogInfractionAsync(pardon, target, enforcer);
        
        return logResult.IsSuccess
            ? Result.FromSuccess()
            : Result.FromError(new InfractionError(SemiSuccessfulAction, logResult));
    }

    /// <summary>
    ///     Loads all active infractions, and enqueues them for processing.
    /// </summary>
    private async Task LoadActiveInfractionsAsync()
    {
        _logger.LogDebug("Loading active infractions...");

        var now = DateTimeOffset.UtcNow;
        
        var infractions = await _mediator.Send(new GetActiveInfractions.Request(_shard.ShardID, _shard.ShardCount));

        _logger.LogDebug("Loaded infractions in {Time:N0} ms.", (DateTimeOffset.UtcNow - now).TotalMilliseconds);

        if (!infractions.Any())
        {
            _logger.LogDebug("No active infractions to handle. Skipping.");
            return;
        }

        _logger.LogDebug("Enqueuing {Infractions} infractions.", infractions.Count());

        _queue.AddRange(infractions);
        
        SilkMetric.LoadedInfractions.IncTo(_queue.Count);
        
    }
    
    /// <summary>
    /// Attempts to create a mute role for a guild.
    /// </summary>
    /// <param name="guildID">The ID of the guild to generate a mute role for.</param>
    private async Task<Result> TryCreateMuteRoleAsync(Snowflake guildID)
    {
        var selfResult = await _guilds.GetCurrentGuildMemberAsync(_users, guildID);
        
        if (!selfResult.IsSuccess)
            return Result.FromError(selfResult.Error);
        
        var guildRolesResult = await _guilds.GetGuildRolesAsync(guildID);
        
        if (!guildRolesResult.IsSuccess)
            return Result.FromError(guildRolesResult.Error);

        var everyoneRole = guildRolesResult.Entity.Single(role => role.ID == guildID);
        
        var botRoles = guildRolesResult.Entity.Where(r => selfResult.Entity.Roles.Contains(r.ID)).ToArray();
        
        var roleResult = await _guilds.CreateGuildRoleAsync(
                                                            guildID, 
                                                            "Muted",
                                                            DiscordPermissionSet.Empty,
                                                            isMentionable: false,
                                                            reason: "Mute role created by AutoMod.");
        
        if (!roleResult.IsSuccess)
            return Result.FromError(new PermissionError("Unable to create mute role."));

        var botPosition = Math.Max(0, botRoles.MaxOrDefault(r => r.Position) - 1);

        var modResult = await _guilds.ModifyGuildRolePositionsAsync(guildID, new[] { (roleResult.Entity.ID, new Optional<int?>(botPosition)) });

        if (!modResult.IsSuccess)
            return Result.FromError(new PermissionError("Unable to modify mute role position."));

        var channels = await _guilds.GetGuildChannelsAsync(guildID);
        
        if (!channels.IsSuccess)
            return Result.FromError(channels.Error);

        foreach (var channel in channels.Entity.Where(c => c.Type is ChannelType.GuildText))
        {
            if (!channel.PermissionOverwrites.IsDefined(out var overwrites))
                overwrites = new List<IPermissionOverwrite>();
            
            var permissions         = DiscordPermissionSet.ComputePermissions(roleResult.Entity.ID, everyoneRole, overwrites);
            var selfPermissions     = DiscordPermissionSet.ComputePermissions(selfResult.Entity.User.Value.ID, everyoneRole, botRoles, overwrites);
            var everyonePermissions = DiscordPermissionSet.ComputePermissions(everyoneRole.ID, everyoneRole, overwrites);

            if (!selfPermissions.HasPermission(DiscordPermission.ManageChannels))
                continue;
            
            if (selfPermissions.HasPermission(DiscordPermission.ViewChannel) ||
                everyonePermissions.HasPermission(DiscordPermission.ViewChannel))
            {
                var overwriteResult = await _channels.EditChannelPermissionsAsync
                    (
                     channel.ID,
                     roleResult.Entity.ID,
                     new DiscordPermissionSet(DiscordPermission.ViewChannel),
                     new DiscordPermissionSet(DiscordPermission.SendMessages, DiscordPermission.AddReactions)
                    );

                if (!overwriteResult.IsSuccess && !overwrites.Any())
                    _logger.LogError("Failed to set permissions in {Channel} on {Guild}, but permissions should allow for it.", channel.ID, guildID);
            }
        }
        
        await _mediator.Send(new UpdateGuildConfig.Request(guildID)
        {
            MuteRoleID = roleResult.Entity.ID
        });
        
        return Result.FromSuccess();
    }

    /// <summary>
    ///     Gets a user-friendly error message for REST errors.
    /// </summary>
    /// <param name="result">The result from a REST request</param>
    /// <param name="actionName">The infraction action what was being performed. </param>
    private IResultError GetActionFailedErrorMessage(Result result, string actionName)
    {
        if (result.Error is not RestResultError<RestError> re)
            return result.Error!;

        return re.Error.Code.OrDefault() switch
        {
            DiscordError.MissingAccess => new PermissionError($"I don't have permission to {actionName} people!"),
            DiscordError.UnknownUser   => new NotFoundError("That user doesn't seem to exist! Are you sure you typed their ID correctly?"),
            DiscordError.UnknownBan    => new NotFoundError("That user doesn't appear to be banned!"),
            _                          => result.Error
        };
    }

    private async Task<Result<(IUser target, IUser enforcer)>> TryGetEnforcerAndTargetAsync(Snowflake guildID, Snowflake targetID, Snowflake enforcerID)
    {
        IUser target;
        IUser enforcer;

        Result<IUser> currentResult = await _users.GetCurrentUserAsync();

        if (!currentResult.IsDefined(out IUser? bot))
        {
            _logger.LogError(EventIds.Service, "Failed to get current bot user.");
            return Result<(IUser target, IUser enforcer)>.FromError(currentResult.Error!);
        }

        Result<IGuildMember> currentMemberResult = await _guilds.GetGuildMemberAsync(guildID, bot.ID);

        if (!currentMemberResult.IsDefined(out IGuildMember? currentMember))
        {
            _logger.LogError(EventIds.Service, "Failed to get current bot member.");
            return Result<(IUser target, IUser enforcer)>.FromError(currentMemberResult.Error!);
        }

        Result<IReadOnlyList<IRole>> rolesResult = await _guilds.GetGuildRolesAsync(guildID);

        if (!rolesResult.IsDefined(out IReadOnlyList<IRole>? roles))
            return Result<(IUser target, IUser enforcer)>.FromError(rolesResult.Error!);

        Result<IGuildMember> enforcerResult = await _guilds.GetGuildMemberAsync(guildID, enforcerID);

        if (!enforcerResult.IsDefined(out IGuildMember? enforcerMember))
            return Result<(IUser target, IUser enforcer)>.FromError(new NotFoundError("Enforcer is not present on the specified guild."));

        enforcer = enforcerMember.User.Value;

        Result<IGuildMember> targetMemberResult = await _guilds.GetGuildMemberAsync(guildID, targetID);

        if (!targetMemberResult.IsDefined(out IGuildMember? targetMember))
        {
            Result<IUser> targetResult = await _users.GetUserAsync(targetID);

            if (!targetResult.IsDefined(out IUser? targetUser))
                return Result<(IUser target, IUser enforcer)>.FromError(new NotFoundError("That user doesn't seem to exist!"));

            target = targetUser;
        }
        else
        {
            IEnumerable<IRole> targetRoles   = roles.Where(r => targetMember.Roles.Contains(r.ID));
            IEnumerable<IRole> enforcerRoles = roles.Where(r => enforcerMember.Roles.Contains(r.ID));
            IEnumerable<IRole> botRoles      = roles.Where(r => currentMember.Roles.Contains(r.ID));

            int targetBotRoleDiff      = targetRoles.MaxOrDefault(r => r.Position) - botRoles.MaxOrDefault(r => r.Position);
            int targetEnforcerRoleDiff = targetRoles.MaxOrDefault(r => r.Position) - enforcerRoles.MaxOrDefault(r => r.Position);

            //Bug?: In the event that another role is added on top, MaxBy should be replaced with Any.
            if (targetEnforcerRoleDiff >= 0 && targetRoles.MaxBy(r => r.Position)?.Name != "Muted")
                return Result<(IUser target, IUser enforcer)>.FromError(new HierarchyError("Their roles are higher or equal to yours! I can't do anything."));

            if (targetBotRoleDiff >= 0)
                return Result<(IUser target, IUser enforcer)>.FromError(new HierarchyError("Their roles are higher or equal to mine! I can't do anything."));

            target = targetMember.User.Value;
        }

        return Result<(IUser target, IUser enforcer)>.FromSuccess((target, enforcer));
    }

    /// <summary>
    ///     Determines whether both the enforcer and bot have permission to act upon the target.
    /// </summary>
    /// <param name="guildID">The ID of the guild to check.</param>
    /// <param name="enforcerID">The ID of the enforcer.</param>
    /// <param name="permission">The permission to check</param>
    private async Task<Result> EnsureHasPermissionsAsync(Snowflake guildID, Snowflake enforcerID, DiscordPermission permission)
    {
        Result<IUser> selfUserResult = await _users.GetCurrentUserAsync();

        if (!selfUserResult.IsDefined(out var selfUser))
            return Result.FromError(selfUserResult.Error!);

        Result<IGuildMember> selfResult = await _guilds.GetGuildMemberAsync(guildID, selfUser.ID);

        if (!selfResult.IsSuccess)
            return Result.FromError(selfResult.Error);

        IGuildMember self = selfResult.Entity;

        Result<IGuildMember> enforcerResult = await _guilds.GetGuildMemberAsync(guildID, enforcerID);

        if (!enforcerResult.IsSuccess)
            return Result.FromError(enforcerResult.Error);

        IGuildMember enforcer = enforcerResult.Entity;

        Result<IReadOnlyList<IRole>> rolesResult = await _guilds.GetGuildRolesAsync(guildID);

        if (!rolesResult.IsSuccess)
            return Result.FromError(rolesResult.Error);

        IReadOnlyList<IRole> roles = rolesResult.Entity;

        IRole[] enforcerRoles = roles.Where(r => enforcer.Roles.Contains(r.ID)).ToArray();
        IRole[] selfRoles     = roles.Where(r => self.Roles.Contains(r.ID)).ToArray();

        var enforcerPermissions = DiscordPermissionSet.ComputePermissions(enforcerID, roles.First(r => r.ID == guildID), enforcerRoles);
        var selfPermissions     = DiscordPermissionSet.ComputePermissions(selfUser.ID, roles.First(r => r.ID == guildID), selfRoles);

        bool enforcerCanAct = enforcerPermissions.HasPermission(permission) || enforcerPermissions.HasPermission(DiscordPermission.Administrator);
        bool selfCanAct     = selfPermissions.HasPermission(permission) || selfPermissions.HasPermission(DiscordPermission.Administrator);

        if (!enforcerCanAct)
            return Result.FromError(new PermissionError($"You don't have permission to {permission.Humanize(LetterCasing.LowerCase)}!"));

        if (!selfCanAct)
            return Result.FromError(new PermissionError($"I don't have permission to {permission.Humanize(LetterCasing.LowerCase)}!"));

        return Result.FromSuccess();
    }

    /// <summary>
    ///     Attempts to log an infraction to a user.
    /// </summary>
    /// <param name="infraction">The infraction to log.</param>
    /// <param name="enforcer">The enforcer responsible for the infraction.</param>
    /// <param name="guildID">The ID of the guild the infraction took place on.</param>
    private async Task<Result<bool>> TryInformTargetAsync(Infraction infraction, IUser enforcer, Snowflake guildID)
    {
        Result<IGuild> guildResult = await _guilds.GetGuildAsync(guildID);

        if (!guildResult.IsDefined(out IGuild? guild))
        {
            _logger.LogError("{Enforcer} attempted to infract {Target} on {Guild} but the guild is not available.", enforcer.ID, infraction.TargetID, guildID);
            return false;
        }

        string action = infraction.Type switch
        {
            InfractionType.Kick    => "kicked",
            InfractionType.Ban     => "banned",
            InfractionType.SoftBan => "temporarily banned",
            InfractionType.Strike  => "warned",
            InfractionType.Pardon  => "pardoned",
            InfractionType.Unban   => "unbanned",
            InfractionType.Unmute  => "unmuted",
            InfractionType.Mute or 
            InfractionType.AutoModMute => "muted",
            
            _ => infraction.Type.ToString()
        };

        var avatar = CDN.GetUserAvatarUrl(enforcer, imageSize: 4096);
        
        var embed = new Embed
        {
            Title       = "Action Reason:",
            Footer      = new EmbedFooter($"{enforcer.Username}#{enforcer.Discriminator:0000}", IconUrl: avatar.Entity?.ToString() ?? default(Optional<string>)),
            Description = infraction.Reason,
            Colour      = Color.Firebrick
        };

        Result<IChannel> channelResult = await _users.CreateDMAsync(infraction.TargetID);

        if (!channelResult.IsDefined(out IChannel? channel))
        {
            _logger.LogError("Failed to create DM channel for user {UserID}.", infraction.TargetID);
            return false;
        }

        var notificationResult = await _channels.CreateMessageAsync(channel.ID, $"You have been **{action}** from **{guild.Name}**.", embeds: new[] { embed });

        return notificationResult.IsSuccess;
    }
    
    /// <summary>
    /// Logs an updated infraction, and who updated it.
    /// </summary>
    /// <param name="infraction">The infraction to update</param>
    /// <param name="updatedBy">Who the infraction was updated by.</param>
    /// <param name="infractionTarget">Who the infraction was toward.</param>
    /// <param name="infractionEnforcer">Who the infraction was enforced by.</param>
    /// <param name="updatedAt">When the infraction was updated.</param>
    private async Task<Result> LogInfractionUpdateAsync(Infraction infraction, IUser updatedBy, IUser infractionTarget, IUser infractionEnforcer, DateTimeOffset updatedAt)
    {
        var config = await _mediator.Send(new GetGuildConfig.Request(infraction.GuildID));

        if (!config.Logging.LogInfractions || config.Logging.Infractions is null)
            return Result.FromSuccess();

        Result channelExists = await EnsureLoggingChannelExistsAsync(infraction.GuildID);

        if (!channelExists.IsSuccess)
            return Result.FromError(new NotFoundError());

        bool useWebhook = config.Logging.UseWebhookLogging;

        var embed = new Embed
        {
            Title       = "Infraction #" + infraction.CaseID,
            Author      = new EmbedAuthor($"{infractionTarget.Username}#{infractionTarget.Discriminator}", CDN.GetUserAvatarUrl(infractionTarget, imageSize: 1024).Entity.ToString()),
            Description = infraction.Reason,
            Colour      = Color.Goldenrod,
            Fields = new IEmbedField[]
            {
                new EmbedField("Type:", infraction.Type.ToString(), true),
                new EmbedField("Infracted at:", $"<t:{(infraction.CreatedAt).ToUnixTimeSeconds()}:F>", true),
                new EmbedField("Expires:", !infraction.ExpiresAt.HasValue ? "Never" : $"<t:{infraction.ExpiresAt.Value.ToUnixTimeSeconds()}:F>", true),

                new EmbedField("Moderator:", $"**{infractionEnforcer.Username}#{infractionEnforcer.Discriminator}**\n(`{infractionEnforcer.ID}`)", true),
                new EmbedField("Offender:", $"**{infractionTarget.Username}#{infractionTarget.Discriminator}**\n(`{infractionTarget.ID}`)", true),

            }
        };
        
        return await _channelLogger.LogAsync(useWebhook, config.Logging.Infractions, $"📝 Case #{infraction.CaseID} was updated by **{updatedBy.Username}**", embed);
    }

    /// <summary>
    ///     Logs a given infraction to the designated logging channel, using a webhook if configured.
    /// </summary>
    /// <param name="infraction">The infraction to log.</param>
    /// <param name="target">The target of the infraction.</param>
    /// <param name="enforcer">The enforcer of the infraction.</param>
    private async Task<Result> LogInfractionAsync(Infraction infraction, IUser target, IUser enforcer)
    {
        var config = await _mediator.Send(new GetGuildConfig.Request(infraction.GuildID));

        if (!config.Logging.LogInfractions || config.Logging.Infractions is null)
            return Result.FromSuccess();

        Result channelExists = await EnsureLoggingChannelExistsAsync(infraction.GuildID);

        if (!channelExists.IsSuccess)
            return Result.FromError(new NotFoundError());

        bool useWebhook = config.Logging.UseWebhookLogging;

        var embed = new Embed
        {
            Title       = "Infraction #" + infraction.CaseID,
            Author      = new EmbedAuthor($"{target.ToDiscordTag()}", default, CDN.GetUserAvatarUrl(target, imageSize: 1024).Entity.ToString()),
            Description = infraction.Reason,
            Colour      = Color.Goldenrod,
            Fields = new IEmbedField[]
            {
                new EmbedField("Type:", infraction.Type.ToString(), true),
                new EmbedField("Infracted at:", $"<t:{infraction.CreatedAt.ToUnixTimeSeconds()}:F>", true),
                new EmbedField("Expires:", !infraction.ExpiresAt.HasValue ? "Never" : $"<t:{((DateTimeOffset)infraction.ExpiresAt).ToUnixTimeSeconds()}:F>", true),
                new EmbedField("Moderator:", $"**{enforcer.ToDiscordTag()}**\n(`{enforcer.ID}`)", true),
                new EmbedField("Offender:", $"**{target.ToDiscordTag()}**\n(`{target.ID}`)", true),
            }
        };

        var logResult = await _channelLogger.LogAsync(useWebhook, config.Logging.Infractions!, null, embed);

        if (!logResult.IsSuccess)
        {
            _logger.LogError("Failed to log infraction for guild {Guild}.", infraction.GuildID);
            return Result.FromError(logResult.Error);
        }

        return Result.FromSuccess();
    }
    
    /// <summary>
    ///     Ensures an available logging channel exists on the guild, creating one if necessary.
    /// </summary>
    /// <param name="guildID"></param>
    private async Task<Result> EnsureLoggingChannelExistsAsync(Snowflake guildID)
    {
        var config = await _mediator.Send(new GetGuildConfig.Request(guildID));

        Debug.Assert(config.Logging.LogInfractions, "Caller should validate that infraction logging is enabled.");

        Result<IUser> currentResult = await _users.GetCurrentUserAsync();

        if (!currentResult.IsSuccess)
        {
            _logger.LogCritical("Failed to get current user.");
            return Result.FromError(currentResult.Error);
        }

        IUser currentUser = currentResult.Entity;

        Result<IGuildMember> currentMemberResult = await _guilds.GetGuildMemberAsync(guildID, currentUser.ID);

        if (!currentMemberResult.IsDefined(out IGuildMember? currentMember))
        {
            _logger.LogCritical("Failed to fetch self from guild.");
            return Result.FromError(currentMemberResult.Error!);
        }
        
        if (config.Logging.Infractions is not { } existingLogChannel)
        {
            return Result.FromSuccess();
        }
        
        var infractionChannelResult = await _channels.GetChannelAsync(existingLogChannel.ChannelID);

        if (!infractionChannelResult.IsSuccess)
        {
            _logger.LogError(EventIds.Service, "Designated infraction channel for {Guild} has gone missing with no fallback.", guildID);
            return Result.FromError(new InvalidOperationError());
        }

        Result<IReadOnlyList<IRole>> rolesResult = await _guilds.GetGuildRolesAsync(guildID);

        if (!rolesResult.IsDefined(out IReadOnlyList<IRole>? roles))
        {
            _logger.LogCritical("Failed to fetch roles from guild.");
            return Result.FromError(rolesResult.Error!);
        }

        IDiscordPermissionSet loggingChannelPermissions = DiscordPermissionSet.ComputePermissions
        (
         currentUser.ID,
         roles.Single(r => r.ID == guildID),
         roles.Where(r => currentMember.Roles.Contains(r.ID)).ToArray()
        );

        if (!loggingChannelPermissions.HasPermission(DiscordPermission.SendMessages))
        {
            _logger.LogInformation("Infraction channel is set, but permissions were changed. Cannot send messages.");

            return Result.FromError(new PermissionDeniedError("An infraction channel was set, but permissions do not allow sending messages."));
        }

        if (!loggingChannelPermissions.HasPermission(DiscordPermission.EmbedLinks))
        {
            _logger.LogInformation("Infraction channel is set, but permissions were changed. Cannot send embeds.");

            return Result.FromError(new PermissionDeniedError("An infraction channel was set, but permissions do not allow embeds."));
        }

        if (config.Logging.UseWebhookLogging)
        {
            if (!loggingChannelPermissions.HasPermission(DiscordPermission.ManageWebhooks))
            {
                _logger.LogInformation("Infraction channel is set, but permissions were changed. Cannot manage webhooks.");

                return Result.FromError(new PermissionDeniedError("An infraction channel was set, but permissions do not allow managing webhooks."));
            }

            if (config.Logging.Infractions.WebhookID.Value is 0)
            {
                _logger.LogDebug("Attempting to create new webhook for infraction channel.");
            }
            else
            {
                Result<IWebhook> webhookResult = await _webhooks.GetWebhookAsync(config.Logging.Infractions.WebhookID);

                if (!webhookResult.IsSuccess)
                {
                    _logger.LogWarning("Webhook has gone missing. Attempting to create a new one.");

                    Result<IWebhook> createWebhookResult = await _webhooks.CreateWebhookAsync(config.Logging.Infractions.ChannelID, SilkWebhookName, default);

                    if (!createWebhookResult.IsSuccess)
                    {
                        _logger.LogCritical("Failed to create webhook for infraction channel. Giving up.");

                        return Result.FromError(createWebhookResult.Error!);
                    }
                    _logger.LogDebug("Successfully created new webhook for infraction channel.");

                    IWebhook webhook = createWebhookResult.Entity;

                    config.Logging.Infractions.WebhookID    = webhook.ID;
                    config.Logging.Infractions.WebhookToken = webhook.Token.Value;

                    await _mediator.Send(new UpdateGuildConfig.Request(guildID) { LoggingConfig = config.Logging });
                }
            }
        }

        return Result.FromSuccess();
    }
}