using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Abstractions.Results;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Results;
using Remora.Rest.Core;
using Remora.Rest.Results;
using Remora.Results;
using Silk.Core.Data.Entities;
using Silk.Core.Data.MediatR.Guilds;
using Silk.Core.Data.MediatR.Infractions;
using Silk.Core.Errors;
using Silk.Core.Services.Data;
using Silk.Core.Services.Interfaces;
using Silk.Core.Types;
using Silk.Extensions;
using Silk.Shared.Constants;

namespace Silk.Core.Services.Server;

public sealed class InfractionService : IHostedService, IInfractionService
{
    private const string SemiSuccessfulAction = "The action completed, but there was an error processing the infraction.";

    private const string SilkWebhookName = "Silk! Logging";
        
    private readonly AsyncTimer _queueTimer;
        
    private readonly ILogger<InfractionService> _logger;
    private readonly IMediator                  _mediator;
        
    private readonly GuildConfigCacheService    _config;

    private readonly IDiscordRestUserAPI    _users;
    private readonly IDiscordRestGuildAPI   _guilds;
    private readonly IDiscordRestChannelAPI _channels;
    private readonly IDiscordRestWebhookAPI _webhooks;

    private readonly ConcurrentBag<InfractionEntity> _queue = new();
    public InfractionService
    (
        ILogger<InfractionService> logger,
        IMediator mediator,
        GuildConfigCacheService config,
        IDiscordRestUserAPI users,
        IDiscordRestGuildAPI guilds,
        IDiscordRestChannelAPI channels,
        IDiscordRestWebhookAPI webhooks
    )
    {
        _logger = logger;
        _mediator = mediator;
        _config = config;
        _users = users;
        _guilds = guilds;
        _channels = channels;
        _webhooks = webhooks;
    }

    /// <inheritdoc/>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting infraction service...");

        await LoadActiveInfractionsAsync();
            
        _queueTimer.Start();
            
        _logger.LogInformation("Infraction service started.");
    }

    /// <inheritdoc/>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping infraction service...");
        _queueTimer.Stop();
        _queue.Clear();
        _logger.LogInformation("Infraction service stopped.");
    }
        
    /// <inheritdoc/>
    public async Task<Result> AutoInfractAsync(Snowflake guildID, Snowflake targetID, Snowflake enforcerID, string reason = "Not Given.") => throw new NotImplementedException();

    /// <inheritdoc/>
    public async Task<Result> UpdateInfractionAsync(InfractionEntity infraction, string? newReason = null, Optional<TimeSpan?> newExpiration = default)
    {
        if (newExpiration.IsDefined(out TimeSpan? expiration))
            if (expiration.Value < TimeSpan.Zero)
                return Result.FromError(new ArgumentOutOfRangeError(nameof(newExpiration), "Expiration cannot be negative."));
            
        await _mediator.Send(new UpdateInfractionRequest(infraction.Id, DateTime.UtcNow + expiration, newReason ?? default(Optional<string>)));
        return Result.FromSuccess();
    }

    /// <inheritdoc/>
    public async Task<Result> StrikeAsync(Snowflake guildID, Snowflake targetID, Snowflake enforcerID, string reason = "Not Given.")
    {
        IUser target;
        IUser enforcer;

        var canInfractResult = await TryGetEnforcerAndTargetAsync(guildID, targetID, enforcerID);
            
        if (!canInfractResult.IsSuccess)
            return Result.FromError(canInfractResult.Error);
            
        (target, enforcer) = canInfractResult.Entity;
            
        var infraction = await _mediator.Send(new CreateInfractionRequest(guildID.Value, targetID.Value, enforcerID.Value, reason, InfractionType.Strike));

        await TryInformTargetAsync(infraction, enforcer, guildID);
            
                
        var returnResult = await LogInfractionAsync(infraction, target, enforcer);
            
        return returnResult.IsSuccess 
            ? Result.FromSuccess() 
            : Result.FromError(new AggregateError(SemiSuccessfulAction, returnResult));
    }

    /// <inheritdoc/>
    public async Task<Result> KickAsync(Snowflake guildID, Snowflake targetID, Snowflake enforcerID, string reason = "Not Given.")
    {
        IUser target;
        IUser enforcer;

        var canInfractResult = await TryGetEnforcerAndTargetAsync(guildID, targetID, enforcerID);
            
        if (!canInfractResult.IsSuccess)
            return Result.FromError(canInfractResult.Error);

        (target, enforcer) = canInfractResult.Entity;
            
        var infraction = await _mediator.Send(new CreateInfractionRequest(guildID.Value, targetID.Value, enforcerID.Value, reason, InfractionType.Kick));

        await TryInformTargetAsync(infraction, target, guildID);
            
        var kickResult = await _guilds.RemoveGuildMemberAsync(guildID, targetID, reason);

        if (!kickResult.IsSuccess)
            return GetActionFailedErrorMessage(kickResult, "kick"); 
            
        var returnResult = await LogInfractionAsync(infraction, target, enforcer);
            
        return returnResult.IsSuccess 
            ? Result.FromSuccess() 
            : Result.FromError(new InfractionError(SemiSuccessfulAction, returnResult));
    }

    /// <inheritdoc/>
    public async Task<Result> BanAsync(Snowflake guildID, Snowflake targetID, Snowflake enforcerID, int days = 0, string reason = "Not Given.", TimeSpan? expirationRelativeToNow = null)
    {            
        IUser target;
        IUser enforcer;

        var permissionResult = await EnsureHasPermissionsAsync(guildID, enforcerID, DiscordPermission.BanMembers);
            
        if (!permissionResult.IsSuccess)
            return Result.FromError(permissionResult.Error);
            
        var hierarchyResult = await TryGetEnforcerAndTargetAsync(guildID, targetID, enforcerID);
            
        if (!hierarchyResult.IsSuccess)
            return Result.FromError(hierarchyResult.Error);
            
        (target, enforcer) = hierarchyResult.Entity;
            
        var infraction = await _mediator.Send(new CreateInfractionRequest(guildID.Value, targetID.Value, enforcerID.Value, reason, InfractionType.Kick));

        await TryInformTargetAsync(infraction, target, guildID);

        var banResult = await _guilds.CreateGuildBanAsync(guildID, targetID, days, reason);

        if (!banResult.IsSuccess)
            return GetActionFailedErrorMessage(banResult, "ban"); 
            
        var returnResult = await LogInfractionAsync(infraction, target, enforcer);

        return returnResult.IsSuccess 
            ? Result.FromSuccess() 
            : Result.FromError(new InfractionError(SemiSuccessfulAction, returnResult));
    }
        
    /// <inheritdoc/>
    public async Task<Result> UnBanAsync(Snowflake guildID, Snowflake targetID, Snowflake enforcerID, string reason = "Not Given.")
    {
        IUser target;
        IUser enforcer;

        var banResult = await _guilds.RemoveGuildBanAsync(guildID, targetID, reason);
            
        if (!banResult.IsSuccess)
            return GetActionFailedErrorMessage(banResult, "ban (required for unbanning)"); 
            
        var infraction = await _mediator.Send(new CreateInfractionRequest(guildID.Value, targetID.Value, enforcerID.Value, reason, InfractionType.Unban));
            
        var targetEnforcerResult = await TryGetEnforcerAndTargetAsync(guildID, targetID, enforcerID);
            
        if (!targetEnforcerResult.IsSuccess)
            return Result.FromError(targetEnforcerResult.Error);
            
        (target, enforcer) = targetEnforcerResult.Entity;
            
        var returnResult = await LogInfractionAsync(infraction, target, enforcer);
            
        return returnResult.IsSuccess 
            ? Result.FromSuccess() 
            : Result.FromError(new InfractionError(SemiSuccessfulAction, returnResult));
    }
        
    /// <inheritdoc/>
    public async ValueTask<bool> IsMutedAsync(Snowflake guildID, Snowflake targetID) => false;
        
    /// <inheritdoc/>
    public async Task<Result> MuteAsync(Snowflake guildID, Snowflake targetID, Snowflake enforcerID, string reason = "Not Given.", TimeSpan? expirationRelativeToNow = null) => throw new NotImplementedException();
        
    /// <inheritdoc/>
    public async Task<Result> UnMuteAsync(Snowflake guildID, Snowflake targetID, Snowflake enforcerID, string reason = "Not Given.") => throw new NotImplementedException();
       
    /// <inheritdoc/>
    public async Task<Result> AddNoteAsync(Snowflake guildID, Snowflake targetID, Snowflake enforcerID, string note) => throw new NotImplementedException();
        
    /// <inheritdoc/>
    public async Task<Result> PardonAsync(Snowflake guildID, Snowflake targetID, Snowflake enforcerID, int    caseID, string reason = "Not Given.") => throw new NotImplementedException();
        
    /// <summary>
    /// Loads all active infractions, and enqueues them for processing.
    /// </summary>
    private async Task LoadActiveInfractionsAsync()
    {
        _logger.LogDebug("Loading active infractions...");
            
        var now = DateTimeOffset.UtcNow;
        var infractions = await _mediator.Send(new GetActiveInfractionsRequest());

        _logger.LogDebug("Loaded infractions in {Time} ms.", (DateTimeOffset.UtcNow - now).TotalMilliseconds);
            
        if (!infractions.Any())
        {
            _logger.LogDebug("No active infrations to handle. Skipping.");
            return;
        }
            
        _logger.LogDebug("Enqueuing {Infractions} infractions.", infractions.Count());
            
        foreach (var infraction in infractions)
            _queue.Add(infraction);
    }


    /// <summary>
    /// Gets a user-friendly error message for REST errors.
    /// </summary>
    /// <param name="result">The result from a REST request</param>
    /// <param name="actionName">The infraction action what was being performed. </param>
    private Result GetActionFailedErrorMessage(Result result, string actionName)
    {
        if (result.Error is not RestResultError<RestError> re)
            return result;

        return re.Error.Code switch
        {
            DiscordError.MissingAccess => Result.FromError(new PermissionError($"I don't have permission to {actionName} people!")),
            DiscordError.UnknownUser   => Result.FromError(new NotFoundError("That user doesn't seem to exist! Are you sure you typed their ID correctly?")),
            DiscordError.UnknownBan  => Result.FromError(new NotFoundError("That ban doesn't seem to exist! Are you sure you typed their ID correctly?")),
            _                          => result
        };
    }

    private async Task<Result<(IUser target, IUser enforcer)>> TryGetEnforcerAndTargetAsync(Snowflake guildID, Snowflake targetID, Snowflake enforcerID)
    {
        IUser target;
        IUser enforcer;

        var currentResult = await _users.GetCurrentUserAsync();

        if (!currentResult.IsDefined(out var bot))
        {
            _logger.LogError(EventIds.Service, "Failed to get current bot user.");
            return Result<(IUser target, IUser enforcer)>.FromError(currentResult.Error!);
        }
            
        var currentMemberResult = await _guilds.GetGuildMemberAsync(guildID, bot.ID);
            
        if (!currentMemberResult.IsDefined(out var currentMember))
        {
            _logger.LogError(EventIds.Service, "Failed to get current bot member.");
            return Result<(IUser target, IUser enforcer)>.FromError(currentMemberResult.Error!);
        }
            
        var rolesResult = await _guilds.GetGuildRolesAsync(guildID);
            
        if (!rolesResult.IsDefined(out var roles))
            return Result<(IUser target, IUser enforcer)>.FromError(rolesResult.Error!);
            
        var enforcerResult = await _guilds.GetGuildMemberAsync(guildID, enforcerID);
            
        if (!enforcerResult.IsDefined(out var enforcerMember))
            return Result<(IUser target, IUser enforcer)>.FromError(new NotFoundError("Enforcer is not present on the specified guild."));

        enforcer = enforcerMember.User.Value;
            
        var targetMemberResult = await _guilds.GetGuildMemberAsync(guildID, targetID);

        if (!targetMemberResult.IsDefined(out var targetMember))
        {
            var targetResult = await _users.GetUserAsync(targetID);

            if (!targetResult.IsDefined(out var targetUser))
                return Result<(IUser target, IUser enforcer)>.FromError(new NotFoundError("Target does not exist."));
                
            target = targetUser;
        }
        else
        {
            var targetRoles = roles.Where(r => targetMember.Roles.Contains(r.ID));
            var enforcerRoles = roles.Where(r => enforcerMember.Roles.Contains(r.ID));
            var botRoles = roles.Where(r => currentMember.Roles.Contains(r.ID));
                
            var targetBotRoleDiff =  targetRoles.MaxOrDefault(r => r.Position)     - botRoles.MaxOrDefault(r => r.Position);
            var targetEnforcerRoleDiff = targetRoles.MaxOrDefault(r => r.Position) - enforcerRoles.MaxOrDefault(r => r.Position);
                
            if (targetEnforcerRoleDiff >= 0)
                return Result<(IUser target, IUser enforcer)>.FromError(new HierarchyError("Their roles are higher or equal to yours! I can't do anything."));

            if (targetBotRoleDiff >= 0)
                return Result<(IUser target, IUser enforcer)>.FromError(new HierarchyError("Their roles are higher or equal to mine! I can't do anything."));
                
            target = targetMember.User.Value;
        }
            
        return Result<(IUser target, IUser enforcer)>.FromSuccess((target, enforcer));
    }
    
    /// <summary>
    /// Determines whether both the enforcer and bot have permission to act upon the target.
    /// </summary>
    /// <param name="guildID">The ID of the guild to check.</param>
    /// <param name="enforcerID">The ID of the enforcer.</param>
    /// <param name="permission">The permission to check</param>
    private async Task<Result> EnsureHasPermissionsAsync(Snowflake guildID, Snowflake enforcerID, DiscordPermission permission)
    {
        var selfUserResult = await _users.GetCurrentUserAsync();
            
        if (!selfUserResult.IsSuccess)
            return Result.FromError(selfUserResult.Error);

        var selfResult = await _guilds.GetGuildMemberAsync(guildID, selfUserResult.Entity.ID);
            
        if (!selfResult.IsSuccess)
            return Result.FromError(selfResult.Error);
            
        var self = selfResult.Entity;
            
        var enforcerResult = await _guilds.GetGuildMemberAsync(guildID, enforcerID);
            
        if (!enforcerResult.IsSuccess)
            return Result.FromError(enforcerResult.Error);
            
        var enforcer = enforcerResult.Entity;
            
        var rolesResult = await _guilds.GetGuildRolesAsync(guildID);
            
        if (!rolesResult.IsSuccess)
            return Result.FromError(rolesResult.Error);
            
        var roles = rolesResult.Entity;
            
        var enforcerRoles = roles.Where(r => enforcer.Roles.Contains(r.ID));
        var selfRoles = roles.Where(r => self.Roles.Contains(r.ID));
            
        var enforcerCanAct = enforcerRoles.Any(r => r.Permissions.HasPermission(permission));
        var selfCanAct = selfRoles.Any(r => r.Permissions.HasPermission(permission));

        if (!enforcerCanAct)
            return Result.FromError(new PermissionError($"You don't have permission to {permission.Humanize(LetterCasing.LowerCase)}!"));
            
        if (!selfCanAct)
            return Result.FromError(new PermissionError($"I don't have permission to {permission.Humanize(LetterCasing.LowerCase)}!"));
            
        return Result.FromSuccess();
    }
        
    /// <summary>
    /// Attempts to log an infraction to a user.
    /// </summary>
    /// <param name="infraction">The infraction to log.</param>
    /// <param name="enforcer">The enforcer responsible for the infraction.</param>
    /// <param name="guildID">The ID of the guild the infraction took place on.</param>
    private async Task TryInformTargetAsync(InfractionEntity infraction, IUser enforcer, Snowflake guildID)
    {
        var guildResult = await _guilds.GetGuildAsync(guildID);

        if (!guildResult.IsDefined(out var guild))
        {
            _logger.LogError("Failed to fetch guild {GuildID} for infraction {InfractionID}.", guildID, infraction.Id);
            return;
        }
            
        var action = infraction.InfractionType switch
        {
            InfractionType.Mute or InfractionType.AutoModMute => "muted",
            InfractionType.Kick => "kicked",
            InfractionType.Ban  => "banned",
            InfractionType.SoftBan => "temporarily banned",
            InfractionType.Strike => "warned",
            InfractionType.Pardon => "pardoned",
            InfractionType.Unban => "unbanned",
            InfractionType.Unmute => "unmuted",
                
            _ => infraction.InfractionType.ToString()
        };

        var embed = new Embed()
        {
            Title = "Infraction:",
            Author = new EmbedAuthor($"{enforcer.Username}#{enforcer.Discriminator}"),
            Description = infraction.Reason,
            Colour = Color.Firebrick
        };

        var channelResult = await _users.CreateDMAsync(new(infraction.UserId));
            
        if (!channelResult.IsDefined(out var channel))
        {
            _logger.LogError("Failed to create DM channel for user {UserID} for infraction {InfractionID}.", infraction.UserId, infraction.Id);
            return;
        }

        await _channels.CreateMessageAsync(channel.ID, $"You have been **{action}** from **{guild.Name}**.", embeds: new[] { embed });
    }
        
    /// <summary>
    /// Logs a given infraction to the designated logging channel, using a webhook if configured.
    /// </summary>
    /// <param name="infraction">The infraction to log.</param>
    /// <param name="target">The target of the infraction.</param>
    /// <param name="enforcer">The enforcer of the infraction.</param>
    private async Task<Result> LogInfractionAsync(InfractionEntity infraction, IUser target, IUser enforcer)
    {
        var config = await _config.GetModConfigAsync(infraction.GuildId);
            
        if (!config.LoggingConfig.LogInfractions)
            return Result.FromSuccess();

        var channelExists = await EnsureLoggingChannelExistsAsync(new(infraction.GuildId));
            
        if (!channelExists.IsSuccess)
            return Result.FromError(new NotFoundError());
            
        bool useWebhook = config.LoggingConfig.UseWebhookLogging;

        var embed = new Embed
        {
            Title = "Infraction #" + infraction.CaseNumber,
            Author = new EmbedAuthor($"{target.Username}#{target.Discriminator}", CDN.GetUserAvatarUrl(target, imageSize: 1024).Entity.ToString()),
            Description = infraction.Reason,
            Colour = Color.Goldenrod,
            Fields = new IEmbedField[]
            {
                new EmbedField("Type:", infraction.InfractionType.ToString(), true),
                new EmbedField("Infracted at:", $"<t:{((DateTimeOffset)infraction.InfractionTime).ToUnixTimeSeconds()}:F>", true),
                new EmbedField("Expires:", !infraction.Expiration.HasValue ?"Never" :  $"<t:{((DateTimeOffset)infraction.Expiration).ToUnixTimeSeconds()}:F>", true),
                    
                new EmbedField("Moderator:", $"**{enforcer.Username}#{enforcer.Discriminator}**\n(`{enforcer.ID}`)", true),
                new EmbedField("Offender:", $"**{target.Username}#{target.Discriminator}**\n(`{target.ID}`)", true),
                    
            }
        };

        Result<IMessage> logResult;

        if (useWebhook)
        {
            logResult = (await _webhooks.ExecuteWebhookAsync(
                new(config.LoggingConfig.Infractions!.WebhookId),
                config.LoggingConfig.Infractions.WebhookToken,
                embeds: new [] { embed }))!;
        }
        else
        {
            logResult = await _channels.CreateMessageAsync(new (config.LoggingConfig.Infractions!.ChannelId), embeds: new [] { embed });
        }

        if (!logResult.IsSuccess)
        {
            _logger.LogError("Failed to log infraction for guild {Guild}.", infraction.GuildId);
            return Result.FromError(logResult.Error);
        }
            
        return Result.FromSuccess();
    }
        
        
    /// <summary>
    /// Ensures an available logging channel exists on the guild, creating one if neccecary.
    /// </summary>
    /// <param name="guildID"></param>
    private async Task<Result> EnsureLoggingChannelExistsAsync(Snowflake guildID)
    {
        var config = await _config.GetModConfigAsync(guildID.Value);

        Debug.Assert(config.LoggingConfig.LogInfractions, "Caller should validate that infraction logging is enabled.");
            
        var currentResult = await _users.GetCurrentUserAsync();
            
        if (!currentResult.IsSuccess)
        {
            _logger.LogCritical("Failed to get current user.");
            return Result.FromError(currentResult.Error);
        }
            
        var currentUser = currentResult.Entity;

        var currentMemberResult = await _guilds.GetGuildMemberAsync(guildID, currentUser.ID);
            
        if (!currentMemberResult.IsDefined(out var currentMember))
        {
            _logger.LogCritical("Failed to fetch self from guild.");
            return Result.FromError(currentMemberResult.Error!);
        }
            

        if (config.LoggingConfig.Infractions is not LoggingChannelEntity ilc)
        {
            return Result.FromSuccess();
        }
        else
        {
            var infractionChannelResult = await _channels.GetChannelAsync(new(ilc.ChannelId));

            if (!infractionChannelResult.IsSuccess)
            {
                _logger.LogError(EventIds.Service, "Designated infraction channel for {Guild} has gone missing with no fallback.", guildID);
                return Result.FromError(new InvalidOperationError());
            }
        }

        var rolesResult = await _guilds.GetGuildRolesAsync(guildID);
            
        if (!rolesResult.IsDefined(out var roles))
        {
            _logger.LogCritical("Failed to fetch roles from guild.");
            return Result.FromError(rolesResult.Error!);
        }

        var loggingChannelPermissions = DiscordPermissionSet.ComputePermissions
        (
            currentUser.ID,
            roles.Single(r => r.ID == guildID),
            roles.Where(r => currentMember.Roles.Contains(r.ID)).ToArray()
        );

        //TODO: Log errors to DB
            
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

        if (config.LoggingConfig.UseWebhookLogging)
        {
            if (!loggingChannelPermissions.HasPermission(DiscordPermission.ManageWebhooks))
            {
                _logger.LogInformation("Infraction channel is set, but permissions were changed. Cannot manage webhooks.");
                    
                return Result.FromError(new PermissionDeniedError("An infraction channel was set, but permissions do not allow managing webhooks."));
            }

            if (config.LoggingConfig.Infractions.WebhookId is 0)
            {
                _logger.LogDebug("Attempting to create new webhook for infraction channel.");
                    
            }
            else
            {
                var webhookResult = await _webhooks.GetWebhookAsync(new (config.LoggingConfig.Infractions.WebhookId));

                if (!webhookResult.IsSuccess)
                {
                    _logger.LogWarning("Webhook has gone missing. Attempting to create a new one.");
                        
                    var webhookReuslt = await _webhooks.CreateWebhookAsync(new(config.LoggingConfig.Infractions.ChannelId), SilkWebhookName, default);
                        
                    if (!webhookReuslt.IsSuccess)
                    {
                        _logger.LogCritical("Failed to create webhook for infraction channel. Giving up.");
                            
                        return Result.FromError(webhookReuslt.Error!);
                    }
                    else
                    {
                        _logger.LogDebug("Successfully created new webhook for infraction channel.");

                        var webhook = webhookReuslt.Entity;

                        config.LoggingConfig.Infractions.WebhookId = webhook.ID.Value;
                        config.LoggingConfig.Infractions.WebhookToken = webhook.Token.Value;
                            
                        await _mediator.Send(new UpdateGuildModConfigRequest(guildID.Value) { LoggingConfig = config.LoggingConfig });
                    }
                }
            }
        }
            
        return Result.FromSuccess();
    }
}