using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Guilds;

public static class UpdateGuildConfig
{
    public record Request(Snowflake GuildID) : IRequest<GuildConfigEntity>
    {
        public bool                                      ShouldCommit           { get; init; } = true;
        public Optional<bool>                            DetectRaids            { get; init; }
        public Optional<bool>                            ScanInvites            { get; init; }
        public Optional<Snowflake>                       MuteRoleID             { get; init; }
        public Optional<int>                             RaidCooldown           { get; init; }
        public Optional<int>                             RaidThreshold          { get; init; }
        public Optional<bool>                            UseNativeMute          { get; init; }
        public Optional<int>                             MaxUserMentions        { get; init; }
        public Optional<int>                             MaxRoleMentions        { get; init; }
        public Optional<bool>                            BlacklistInvites       { get; init; }
        public Optional<bool>                            UseAggressiveRegex     { get; init; }
        public Optional<bool>                            EscalateInfractions    { get; init; }
        public Optional<bool>                            WarnOnMatchedInvite    { get; init; }
        public Optional<bool>                            DetectPhishingLinks    { get; init; }
        public Optional<bool>                            DeletePhishingLinks    { get; init; }
        public Optional<bool>                            DeleteOnMatchedInvite  { get; init; }
        public Optional<bool>                            BanSuspiciousUsernames { get; init; }
        public Optional<List<GuildGreetingEntity>>       Greetings              { get; init; }
        public Optional<GuildLoggingConfigEntity>        LoggingConfig          { get; init; }
        public Optional<List<InviteEntity>>              AllowedInvites         { get; init; }
        public Optional<List<ExemptionEntity>>           Exemptions             { get; init; }
        public Optional<List<InfractionStepEntity>>      InfractionSteps        { get; init; }
        public Dictionary<string, InfractionStepEntity>? NamedInfractionSteps   { get; init; }
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class Handler : IRequestHandler<Request, GuildConfigEntity>
    {
       private readonly IMediator    _mediator;
       private readonly IDbContextFactory<GuildContext> _dbFactory;

        public Handler(IDbContextFactory<GuildContext> dbFactory, IMediator mediator)
        {
            _dbFactory = dbFactory;
            _mediator  = mediator;
        }

        public async ValueTask<GuildConfigEntity> Handle(Request request, CancellationToken cancellationToken)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            
            var config = db.GuildConfigs.Local.FirstOrDefault(x => x.GuildID == request.GuildID) 
                      ?? await _mediator.Send(new GetGuildConfig.Request(request.GuildID, true), cancellationToken);

            if (config is null)
                throw new KeyNotFoundException($"Guild config for a guild with Id: {request.GuildID} was not found");

            if (request.DetectRaids.IsDefined(out var detect))
                config.EnableRaidDetection = detect;
            
            if (request.RaidThreshold.IsDefined(out var threshold))
                config.RaidDetectionThreshold = threshold;
            
            if (request.RaidCooldown.IsDefined(out var cooldown))
                config.RaidCooldownSeconds = cooldown;
            
            if (request.Greetings.IsDefined(out var greetings))
            {
                db.RemoveRange(config.Greetings.Except(greetings));
                config.Greetings = greetings;
            }
            
            if (request.MuteRoleID.IsDefined(out var muteRole))
                config.MuteRoleID = muteRole;
            
            if (request.UseNativeMute.IsDefined(out var useNativeMute))
                config.UseNativeMute = useNativeMute;

            if (request.EscalateInfractions.IsDefined(out var escalate))
                config.ProgressiveStriking = escalate;

            if (request.MaxUserMentions.IsDefined(out var maxUserMentions))
                config.MaxUserMentions = maxUserMentions;

            if (request.MaxRoleMentions.IsDefined(out var maxRoleMentions))
                config.MaxRoleMentions = maxRoleMentions;

            if (request.UseAggressiveRegex.IsDefined(out var useAggressiveRegex))
                config.Invites.UseAggressiveRegex = useAggressiveRegex;

            if (request.ScanInvites.IsDefined(out var scanInvites))
                config.Invites.ScanOrigin = scanInvites;

            if (request.DeletePhishingLinks.IsDefined(out var deletePhishingLinks))
                config.DeletePhishingLinks = deletePhishingLinks;

            if (request.DetectPhishingLinks.IsDefined(out var detectPhishingLinks))
                config.DetectPhishingLinks = detectPhishingLinks;

            if (request.BlacklistInvites.IsDefined(out var blacklistInvites))
                config.Invites.WhitelistEnabled = blacklistInvites;

            if (request.WarnOnMatchedInvite.IsDefined(out var warnOnMatchedInvite))
                config.Invites.WarnOnMatch = warnOnMatchedInvite;

            if (request.DeleteOnMatchedInvite.IsDefined(out var deleteOnMatchedInvite))
                config.Invites.DeleteOnMatch = deleteOnMatchedInvite;
            
            if (request.BanSuspiciousUsernames.IsDefined(out var banSuspiciousUsernames))
                config.BanSuspiciousUsernames = banSuspiciousUsernames;

            if (request.LoggingConfig.IsDefined(out var loggingConfig))
            {
                var log = config.Logging;
                
                log.LogInfractions = loggingConfig.LogInfractions;
                log.Infractions = loggingConfig.Infractions;
                
                log.LogMemberJoins = loggingConfig.LogMemberJoins;
                log.MemberJoins = loggingConfig.MemberJoins;
                
                log.LogMemberLeaves = loggingConfig.LogMemberLeaves;
                log.MemberLeaves = loggingConfig.MemberLeaves;
                
                log.LogMessageEdits = loggingConfig.LogMessageEdits;
                log.MessageEdits = loggingConfig.MessageEdits;
                
                log.LogMessageDeletes = loggingConfig.LogMessageDeletes;
                log.MessageDeletes = loggingConfig.MessageDeletes;
                
                log.UseWebhookLogging = loggingConfig.UseWebhookLogging;
                log.UseMobileFriendlyLogging = loggingConfig.UseMobileFriendlyLogging;
            }
            
            if (request.Exemptions.IsDefined(out var exemptions))
            {
                db.RemoveRange(config.Exemptions.Except(exemptions));
                config.Exemptions = exemptions;
            }

            config.NamedInfractionSteps = request.NamedInfractionSteps ?? config.NamedInfractionSteps;

            if (request.InfractionSteps.IsDefined(out var infractionSteps))
            {
                db.RemoveRange(config.InfractionSteps.Except(infractionSteps));
                config.InfractionSteps = infractionSteps;
            }

            if (request.AllowedInvites.IsDefined(out var whitelistedInvites))
            {
                db.RemoveRange(config.Invites.Whitelist.Except(whitelistedInvites));
                config.Invites.Whitelist = whitelistedInvites;
            }
            
            if (request.ShouldCommit)
                await db.SaveChangesAsync(cancellationToken);
            
            return config;
        }
    }
}
