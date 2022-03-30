using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Humanizer;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OneOf;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Results;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Data.Entities;
using Silk.Data.MediatR.Guilds;
using Silk.Data.MediatR.Infractions;
using Silk.Errors;
using Silk.Extensions;
using Silk.Extensions.Remora;
using Silk.Services.Data;
using Silk.Services.Interfaces;
using Channel = System.Threading.Channels.Channel;

namespace Silk.Services.Guild;

public class InfractionServiceV2 : IInfractionServiceV2, IHostedService
{
    private const string InternalInfractionErrorMessage = "Hmm. Seem's that's a no-go. `{0}` was the error.";

    private static readonly TimeSpan MaxTimeout = TimeSpan.FromDays(28);
    
    private CancellationTokenSource _cts;

    private readonly IMediator                    _mediator;
    private readonly IDiscordRestUserAPI          _users;
    private readonly IDiscordRestGuildAPI         _guilds;
    private readonly IDiscordRestChannelAPI       _channels;
    private readonly IChannelLoggingService       _logging;
    private readonly GuildConfigCacheService      _config;
    private readonly ILogger<InfractionServiceV2> _logger;

    private readonly Channel<InfractionEntity> _infractionQueue;
    
    private readonly Channel<(InfractionRequest, TaskCompletionSource<Result<InfractionEntity>>)> _submittedInfractions;

    public InfractionServiceV2
    (
            IMediator mediator,
            IDiscordRestUserAPI users,
            IDiscordRestGuildAPI guilds,
            IDiscordRestChannelAPI channels,
            IChannelLoggingService logging,
            GuildConfigCacheService config,
            ILogger<InfractionServiceV2> logger
    )
    {
        _mediator = mediator;
        _users    = users;
        _guilds   = guilds;
        _channels = channels;
        _logging  = logging;
        _config   = config;
        _logger   = logger;

        //Handled by HandleSubmittedInfractionsAsync, which means single-producer/single-consumer.
        _infractionQueue = Channel.CreateUnbounded<InfractionEntity>(new() { SingleReader = true, SingleWriter = true });
        
        // Infractions can be submitted concurrently, and from anywhere, so multi-producer/single-consumer.
        _submittedInfractions = Channel.CreateUnbounded<(InfractionRequest, TaskCompletionSource<Result<InfractionEntity>>)>(new() { SingleReader = true });
    }

    public Task<Result<InfractionEntity>> SubmitInfractionAsync(InfractionRequest request)
    {
        var tcs = new TaskCompletionSource<Result<InfractionEntity>>();
        
        // Multi-producer on an unbounded channel always returns true*.
        _submittedInfractions.Writer.TryWrite((request, tcs));
        
        return tcs.Task;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Infraction Service");
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var infractions = await _mediator.Send(new GetActiveInfractions.Request(), _cts.Token);
        
        foreach (var infraction in infractions)
            _infractionQueue.Writer.TryWrite(infraction);
        
        _logger.LogInformation("Loaded {InfractionCount} infractions", infractions.Count());
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_submittedInfractions.Reader.TryPeek(out _))
            _logger.LogDebug("Processing remaining infractions before shutdown");
        
        while (_submittedInfractions.Reader.TryPeek(out _))
            await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
        
        _logger.LogInformation("Stopping Infraction Service");
        
        _cts.Cancel();

        _infractionQueue.Writer.TryComplete();
        _submittedInfractions.Writer.TryComplete();
        
        _logger.LogInformation("Infraction Service Stopped");
    }

    private async Task HandleInfractionsAsync()
    {
        var token = _cts.Token;

        while (!token.IsCancellationRequested)
        {
            var (request, tcs) = await _submittedInfractions.Reader.ReadAsync(token);
            
            var isAutomod             = request.Moderator is null;
            var resolvedRequestResult = await ResolveInfractionRequestAsync(request);
            
            if (!resolvedRequestResult.IsSuccess)
            {
                _logger.LogError(resolvedRequestResult.Error.Message);
                
                tcs.SetResult(Result<InfractionEntity>.FromError(resolvedRequestResult));
            }
            
            request = resolvedRequestResult.Entity;

            var hierarchyResult = await EnsureHierarchyAsync(isAutomod, request.GuildID, request.Target.AsT0, request.Moderator.Value.AsT0, request.Type.Value);

            if (!hierarchyResult.IsSuccess)
            {
                if (hierarchyResult.Error is not HierarchyError)
                    _logger.LogError(hierarchyResult.Error.Message);
                
                tcs.SetResult(Result<InfractionEntity>.FromError(hierarchyResult));
            }

            var notified = false;

            if (request.Notify)
            {
                var notifyResult = await NotifyTargetAsync
                    (
                     request.Target.AsT0.ID,
                     request.Moderator.Value.AsT0,
                     request.GuildID,
                     request.Type.Value,
                     request.UserReason ?? request.Reason,
                     request.Duration
                    );
                
                notified = notifyResult.IsSuccess;
            }

            var infraction = await _mediator.Send
                (
                 new CreateInfraction.Request
                     (
                      request.GuildID,
                      request.Target.AsT0.ID,
                      request.Moderator.Value.AsT0.ID,
                      request.Reason,
                      request.Type.Value,
                      DateTimeOffset.UtcNow + request.Duration,
                      !request.Notify || notified
                     ),
                 _cts.Token
                );
            
            tcs.SetResult(infraction);

            var config = await _config.GetModConfigAsync(request.GuildID);

            if (infraction.Type is InfractionType.Mute && !config.UseNativeMute)
            {
                var muteExists = await EnsureMuteRoleExistsAsync(request.GuildID);

                if (!muteExists.IsDefined(out var configuredConfig))
                    continue;

                config = configuredConfig;
            }

            var infractionResult = await (infraction.Type switch
            {
                InfractionType.Mute => config.UseNativeMute
                    ? _guilds.ModifyGuildMemberAsync(request.GuildID, request.Target.AsT0.ID, communicationDisabledUntil: DateTimeOffset.UtcNow + (request.Duration ?? MaxTimeout), ct: _cts.Token)
                    : _guilds.AddGuildMemberRoleAsync(request.GuildID, request.Target.AsT0.ID, config.MuteRoleID, request.Reason, _cts.Token),

                InfractionType.Unmute => config.UseNativeMute
                    ? _guilds.ModifyGuildMemberAsync(request.GuildID, request.Target.AsT0.ID, communicationDisabledUntil: null, ct: _cts.Token)
                    : _guilds.RemoveGuildMemberRoleAsync(request.GuildID, request.Target.AsT0.ID, config.MuteRoleID, request.Reason, _cts.Token),

                InfractionType.Ban   => _guilds.CreateGuildBanAsync(request.GuildID, request.Target.AsT0.ID, 0, request.Reason, _cts.Token),
                InfractionType.Unban => _guilds.RemoveGuildBanAsync(request.GuildID, request.Target.AsT0.ID, request.Reason, _cts.Token),

                InfractionType.Kick => _guilds.RemoveGuildMemberAsync(request.GuildID, request.Target.AsT0.ID, request.Reason, _cts.Token),
                _                   => Task.FromResult(Result.FromSuccess())
            });

            if (!config.Logging.LogInfractions)
                continue;

            var offender = request.Target.AsT0;
            var moderator = request.Moderator.Value.AsT0;

            var embed = new Embed
            {
                Author      = new EmbedAuthor(offender.ToDiscordTag(), default, CDN.GetUserAvatarUrl(offender).Entity.ToString()),
                Colour      = Color.Gold,
                Description = request.Reason,
                Fields = new EmbedField[]
                {
                    new("Type:", request.Type.Value.ToString(), true),
                    new("Infracted at:", infraction.CreatedAt.ToTimestamp(TimestampFormat.LongDateTime), true),
                    new("Expires:", infraction.ExpiresAt.HasValue ? infraction.ExpiresAt.Value.ToTimestamp() : "Never", true),
                    new("Moderator:", $"**{moderator.ToDiscordTag()}**\n(`{moderator.ID}`)", true),
                    new("Offender:", $"**{offender.ToDiscordTag()}**\n(`{offender.ID}`)", true),
                    new("Notified:", request.Notify ? infraction.UserNotified ? "Yes" : "Non-applicable" : "No", true),
                    new("Status:", infractionResult.IsSuccess ? "Succeeded" : $"Failed:\n{infractionResult.Error.Message}", true)
                }
            };

            await _logging.LogAsync(config.Logging.UseWebhookLogging, config.Logging.Infractions!, embedContent: embed);
        }
    }

    private async Task<Result<InfractionRequest>> ResolveInfractionRequestAsync(InfractionRequest request)
    {
        if (request.Target.IsT1)
        {
            var targetResult = await _users.GetUserAsync(request.Target.AsT1, _cts.Token);
            
            if (!targetResult.IsSuccess)
                return Result<InfractionRequest>.FromError(new NotFoundError("Failed to resolve infraction target"));

            request = request with { Target = OneOf<IUser, Snowflake>.FromT0(targetResult.Entity) };
        }

        if (request.Moderator is not {} nonAutomodEnforcer)
        {
            var selfResult = await _users.GetCurrentUserAsync(_cts.Token);
            
            if (!selfResult.IsSuccess)
                return Result<InfractionRequest>.FromError(new NotFoundError("Failed to resolve infraction moderator"));
            
            request = request with { Moderator = OneOf<IUser, Snowflake>.FromT0(selfResult.Entity) };
        }
        else if (nonAutomodEnforcer.IsT1)
        {
            var enforcerResult = await _users.GetUserAsync(nonAutomodEnforcer.AsT1, _cts.Token);
            
            if (!enforcerResult.IsSuccess)
                return Result<InfractionRequest>.FromError(new NotFoundError("Failed to resolve infraction enforcer"));
            
            request = request with { Moderator = OneOf<IUser, Snowflake>.FromT0(enforcerResult.Entity) };
        }

        if (request.Type is null)
        {
            var config = await _config.GetModConfigAsync(request.GuildID);
            
            var currentInfractionStepType = await GetCurrentInfractionStepAsync(request.GuildID, request.Target.AsT0.ID, config.InfractionSteps);

            request = request with { Type = currentInfractionStepType };
        }
        
        return Result<InfractionRequest>.FromSuccess(request);
    }

    private async Task<InfractionType> GetCurrentInfractionStepAsync(Snowflake guildID, Snowflake targetID, IReadOnlyList<InfractionStepEntity> steps)
    {
        if (!steps.Any())
            return InfractionType.Strike;

        var infractions = await _mediator.Send(new GetUserInfractions.Request(guildID, targetID));
        
        //Math.Min is fine here because we check that there's at least one configured step.
        var infractionStep = Math.Min(infractions.Count(), steps.Count - 1);
        
        return steps[infractionStep].Type;
    }
    

    private async Task<Result> EnsureHierarchyAsync(bool isAutomod, Snowflake guildID, IUser target, IUser enforcer, InfractionType type)
    {
        IUser self;

        if (isAutomod)
        {
            self = enforcer;
        }
        else
        {
            var selfResult = await _users.GetCurrentUserAsync(_cts.Token);

            if (!selfResult.IsSuccess)
                return Result.FromError(new NotFoundError("Failed to resolve self user."));

            self = selfResult.Entity;
        }
        
        var selfMember     =                          await _guilds.GetGuildMemberAsync(guildID, self.ID, _cts.Token);
        var targetMember   =                          await _guilds.GetGuildMemberAsync(guildID, target.ID, _cts.Token);
        var enforcerMember = isAutomod ? selfMember : await _guilds.GetGuildMemberAsync(guildID, enforcer.ID, _cts.Token);

        // If the target isn't present, assume it's
        // safe to infract, and deal with the reprocussions later.
        if (!targetMember.IsSuccess)
            return Result.FromSuccess();

        if (!selfMember.IsSuccess)
            return Result.FromError(new NotFoundError("Failed to resolve self member."));
        
        var rolesResult = await _guilds.GetGuildRolesAsync(guildID, _cts.Token);

        if (!rolesResult.IsDefined(out var roles))
            return Result.FromError(new NotFoundError("Failed to resolve guild roles."));
        
        var selfRoles     =                         roles.Where(r => selfMember.Entity.Roles.Contains(r.ID));
        var targetRoles   =                         roles.Where(r => targetMember.Entity.Roles.Contains(r.ID));
        var enforcerRoles = isAutomod ? selfRoles : roles.Where(r => enforcerMember.Entity.Roles.Contains(r.ID));

        var selfHighestRole = selfRoles.MaxOrDefault(r => r.Position);
        var targetHighestRole = targetRoles.MaxOrDefault(r => r.Position);
        var enforcerHighestRole = enforcerRoles.MaxOrDefault(r => r.Position);

        if (targetHighestRole >= selfHighestRole)
            return Result.FromError(new HierarchyError("Their roles are equal or higher than mine! I can't do anything."));
        
        if (targetHighestRole >= enforcerHighestRole)
            return Result.FromError(new HierarchyError("Your roles are equal or lower than your target! You can't act against them."));

        if (type is not (InfractionType.Kick or InfractionType.Ban or InfractionType.Mute))
            return Result.FromSuccess();

        var config = await _config.GetModConfigAsync(guildID);

        var requiredPermission = type switch
        {
            InfractionType.Ban => DiscordPermission.BanMembers,
            InfractionType.Mute when config.UseNativeMute => DiscordPermission.ModerateMembers,
            InfractionType.Mute => DiscordPermission.ManageRoles,
            InfractionType.Kick => DiscordPermission.KickMembers,

            _ => throw new InvalidOperationException("Heuristically impossible switch arm reached."),
        };
        
        var everyoneRole      = roles.First(r => r.ID == guildID);
        
        var hasSelfPermission = DiscordPermissionSet.ComputePermissions(self.ID, everyoneRole, selfRoles.ToArray());
        var hasEnforcerPermission = isAutomod ? hasSelfPermission : DiscordPermissionSet.ComputePermissions(enforcer.ID, everyoneRole, enforcerRoles.ToArray());
        
        if (!hasSelfPermission.HasPermission(requiredPermission))
            return Result.FromError(new PermissionError($"I'm missing the {requiredPermission.Humanize(LetterCasing.Title)} permission!"));
        
        if (!hasEnforcerPermission.HasPermission(requiredPermission))
            return Result.FromError(new PermissionError($"You're missing the {requiredPermission.Humanize(LetterCasing.Title)} permission!"));

        return Result.FromSuccess();
    }

    private async Task<Result> NotifyTargetAsync(Snowflake userID, IUser enforcer, Snowflake guildID, InfractionType verb, string reason, TimeSpan? expiration)
    {
        var channelResult = await _users.CreateDMAsync(userID, _cts.Token);
        
        if (!channelResult.IsSuccess)
            return Result.FromError(new PermissionDeniedError("Failed to create DM channel."));

        var guildResult = await _guilds.GetGuildAsync(guildID, ct: _cts.Token);
        
        if (!guildResult.IsSuccess)
            return Result.FromError(new NotFoundError("Failed to fetch guild."));

        var verbString = verb switch
        {
            InfractionType.Strike => "__warned__",
            InfractionType.Pardon => "__pardoned__",
            InfractionType.Ban when expiration.HasValue => "temporarily __banned__",
            InfractionType.Ban                          => "__permanently banned__",
            InfractionType.Kick                         => "__kicked__",
            InfractionType.Mute                         => "__muted__",
            InfractionType.AutoModMute                  => "automatically __muted__",
            InfractionType.Unban                        => "__unbanned__",
            InfractionType.Unmute                       => "__unmuted__",
        };

        var embed = new Embed
        {
            Title = $"You have been {verbString}!",
            Author = new EmbedAuthor(enforcer.ToDiscordTag(), IconUrl: CDN.GetUserAvatarUrl(enforcer).Entity.ToString()),
            Description = $"Reason: {reason}",
            Colour = Color.FromArgb(252, 40, 40),
            Fields = new EmbedField[]
            {
                new("Server", guildResult.Entity.Name, true),
                new("Expires", expiration.HasValue ? (DateTimeOffset.UtcNow + expiration).Value.ToTimestamp() : "Never", true)
            }
        };
        
        var sendResult = await _channels.CreateMessageAsync(channelResult.Entity.ID, embeds: new[] { embed }, ct: _cts.Token);

        return sendResult.IsSuccess ? Result.FromSuccess() : Result.FromError(new PermissionDeniedError("Failed to send DM."));
    }

    private async Task<Result<GuildModConfigEntity>> EnsureMuteRoleExistsAsync(Snowflake guildID)
    {
        var config = await _config.GetModConfigAsync(guildID);

        var allRoles = await _guilds.GetGuildRolesAsync(guildID, _cts.Token);

        if (!allRoles.IsSuccess)
        {
            _logger.LogWarning("Failed to fetch guild roles.");
            return Result<GuildModConfigEntity>.FromError(new NotFoundError("Failed to fetch guild roles."));
        }
        
        if (config.MuteRoleID.Value is not 0)
        {


            if (allRoles.Entity.Any(r => r.ID == config.MuteRoleID))
                return Result<GuildModConfigEntity>.FromSuccess(config);
            
            _logger.LogWarning("Mute role configured for {GuildID} has gone missing. Attempting to recreate it.", guildID);
        }
        
        var roleResult = await _guilds.CreateGuildRoleAsync(guildID, "Muted", DiscordPermissionSet.Empty, Color.FromArgb(36, 36, 36), ct: _cts.Token);
        
        if (!roleResult.IsSuccess)
        {
            _logger.LogError("Failed to create mute role in {GuildID}.", guildID);
            return Result<GuildModConfigEntity>.FromError(new PermissionDeniedError("Failed to create mute role."));
        }
        
        var self = await _users.GetCurrentUserAsync(_cts.Token);
        
        if (!self.IsSuccess)
        {
            _logger.LogError("Failed to fetch current user.");
            return Result<GuildModConfigEntity>.FromError(new NotFoundError("Failed to fetch current user."));
        }
        
        var currentMember = await _guilds.GetGuildMemberAsync(guildID, self.Entity.ID, _cts.Token);
        
        if (!currentMember.IsSuccess)
        {
            _logger.LogError("Failed to fetch current member.");
            return Result<GuildModConfigEntity>.FromError(new NotFoundError("Failed to fetch current member."));
        }
        
        var everyoneRole = allRoles.Entity.First(r => r.ID == guildID);
        var selfRoles = allRoles.Entity.Where(r => currentMember.Entity.Roles.Contains(r.ID)).ToArray();
        
        var channels = await _guilds.GetGuildChannelsAsync(guildID, _cts.Token);
        
        if (!channels.IsSuccess)
        {
            _logger.LogError("Failed to fetch guild channels when creating a mute role for {GuildID}.", guildID);
            return Result<GuildModConfigEntity>.FromError(new NotFoundError("Failed to fetch guild channels."));
        }

        var pendingChannels = new List<Task<Result>>();
        
        foreach (var channel in channels.Entity)
        {
            if (!channel.PermissionOverwrites.IsDefined(out var overwrites))
                overwrites = Array.Empty<IPermissionOverwrite>();

            var selfPermissions = DiscordPermissionSet.ComputePermissions(self.Entity.ID, everyoneRole, selfRoles, overwrites);
            
            if (!selfPermissions.HasPermission(DiscordPermission.ManageChannels) || !selfPermissions.HasPermission(DiscordPermission.ViewChannel))
                continue;

            var task = _channels.EditChannelPermissionsAsync
                (
                 channel.ID,
                 roleResult.Entity.ID,
                 new DiscordPermissionSet(DiscordPermission.ViewChannel),
                 new DiscordPermissionSet(DiscordPermission.SendMessages, DiscordPermission.SendMessagesInThreads, DiscordPermission.AddReactions, DiscordPermission.ChangeNickname)
                );
            
            pendingChannels.Add(task);
        }
        
        var results = await Task.WhenAll(pendingChannels);
        
        if (results.Any(r => !r.IsSuccess))
        {
            _logger.LogError("Failed to edit permissions for mute role in {GuildID}.", guildID);
            return Result<GuildModConfigEntity>.FromError(new PermissionDeniedError("Failed to edit permissions."));
        }
        
        await _mediator.Send(new UpdateGuildModConfig.Request(guildID) { MuteRoleID = roleResult.Entity.ID });

        return Result<GuildModConfigEntity>.FromSuccess(config);
    }
}
