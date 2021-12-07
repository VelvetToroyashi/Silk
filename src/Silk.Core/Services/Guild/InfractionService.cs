using System;
using System.Collections.Concurrent;
using System.Diagnostics;
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
using Silk.Core.Data.Entities;
using Silk.Core.Data.MediatR.Infractions;
using Silk.Core.Services.Data;
using Silk.Core.Services.Interfaces;
using Silk.Core.Types;
using Silk.Shared.Constants;

namespace Silk.Core.Services.Server
{
    public sealed class InfractionService : IHostedService, IInfractionService
    {
        private const string SemiSuccsfulAction = "The action completed, but there was an error processing the infraction.";
        
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

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting infraction service...");

            await LoadActiveInfractionsAsync();
            
            _queueTimer.Start();
            
            _logger.LogInformation("Infraction service started.");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping infraction service...");
            _queueTimer.Stop();
            _logger.LogInformation("Infraction service stopped.");
        }
        
        public async Task<Result> AutoInfractAsync(Snowflake guildID, Snowflake targetID, Snowflake enforcerID, string reason = "Not Given.") => default;
        
        public async Task<Result> UpdateInfractionAsync(InfractionEntity infraction, string?   newReson = null, Optional<TimeSpan?> newExpiration = default) => default;
        
        public async Task<Result> StrikeAsync(Snowflake guildID, Snowflake targetID, Snowflake enforcerID, string reason = "Not Given.") => default;
        
        public async Task<Result> KickAsync(Snowflake guildID, Snowflake targetID, Snowflake enforcerID, string reason = "Not Given.") => default;
        
        public async Task<Result> BanAsync(Snowflake guildID, Snowflake targetID, Snowflake enforcerID, string reason = "Not Given.", TimeSpan? expirationRelativeToNow = null) => default;
        
        public async Task<Result> UnBanAsync(Snowflake guildID, Snowflake targetID, Snowflake enforcerID, string reason = "Not Given.") => default;
        
        public async ValueTask<bool> IsMutedAsync(Snowflake guildID, Snowflake targetID) => false;
        
        public async Task<Result> MuteAsync(Snowflake guildID, Snowflake targetID, Snowflake enforcerID, string reason = "Not Given.", TimeSpan? expirationRelativeToNow = null) => default;
        
        public async Task<Result> UnMuteAsync(Snowflake guildID, Snowflake targetID, Snowflake enforcerID, string reason = "Not Given.") => default;
       
        public async Task<Result> AddNoteAsync(Snowflake guildID, Snowflake targetID, Snowflake enforcerID, string note) => default;
        
        public async Task<Result> PardonAsync(Snowflake guildID, Snowflake targetID, Snowflake enforcerID, int    caseID, string reason = "Not Given.") => default;

        
        /// <summary>
        /// Ensures an available logging channel exists on the guild, creating one if neccecary.
        /// </summary>
        /// <param name="guildID"></param>
        /// <returns></returns>
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
            
            IChannel? loggingChannel = default;

            if (config.LoggingConfig.Infractions is LoggingChannelEntity ilc)
            {
                var infractionChannelResult = await _channels.GetChannelAsync(new(ilc.ChannelId));

                if (infractionChannelResult.IsDefined(out var infractionChannel))
                    loggingChannel = infractionChannel;
            }
            else if (config.LoggingConfig.FallbackLoggingChannel is ulong fallback)
            {
                var fallbackChannelResult = await _channels.GetChannelAsync(new(fallback));
                
                if (fallbackChannelResult.IsDefined(out var fallbackChannel))
                    loggingChannel = fallbackChannel;
            }
            else
            {
                _logger.LogError(EventIds.Service, "Designated infraction channel for {Guild} has gone missing with no fallback.", guildID);
                
                return Result.FromError(new InvalidOperationError());
            }

            if (loggingChannel is null)
            {
                _logger.LogError(EventIds.Service, "Infraction channel exists in config, but does not exist in guild.");
                
                return Result.FromError(new NotFoundError("Infraction channel is configured, but does not exist in guild."));
            }

            var rolesResult = await _guilds.GetGuildRolesAsync(guildID);
            
            if (!rolesResult.IsDefined(out var roles))
            {
                _logger.LogCritical("Failed to fetch roles from guild.");
                return Result.FromError(rolesResult.Error!);
            }

            var loggingChannelPermissions = DiscordPermissionSet
                                                                       .ComputePermissions
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
            
            return Result.FromSuccess();
        }
        
        
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
    }
}