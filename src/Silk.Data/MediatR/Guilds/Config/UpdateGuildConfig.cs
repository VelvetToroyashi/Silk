using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Guilds;

public static class UpdateGuildConfig
{
    public record Request(Snowflake GuildID) : IRequest<GuildConfigEntity>
    {
        public Optional<bool>                      ScanInvites            { get; init; }
        public Optional<Snowflake>                 MuteRoleID             { get; init; }
        public Optional<bool>                      UseNativeMute          { get; init; }
        public Optional<int>                       MaxUserMentions        { get; init; }
        public Optional<int>                       MaxRoleMentions        { get; init; }
        public Optional<bool>                      BlacklistInvites       { get; init; }
        public Optional<bool>                      UseAggressiveRegex     { get; init; }
        public Optional<bool>                      EscalateInfractions    { get; init; }
        public Optional<bool>                      WarnOnMatchedInvite    { get; init; }
        public Optional<bool>                      DetectPhishingLinks    { get; init; }
        public Optional<bool>                      DeletePhishingLinks    { get; init; }
        public Optional<bool>                      DeleteOnMatchedInvite  { get; init; }
        public Optional<bool>                      BanSuspiciousUsernames { get; init; }
        public Optional<List<GuildGreetingEntity>> Greetings              { get; init; }

        public Optional<GuildLoggingConfigEntity> LoggingConfig { get; init; }

        public Optional<List<InviteEntity>>              AllowedInvites       { get; init; }
        public Optional<List<ExemptionEntity>>           Exemptions           { get; init; }
        public Optional<List<InfractionStepEntity>>      InfractionSteps      { get; init; }
        public Dictionary<string, InfractionStepEntity>? NamedInfractionSteps { get; init; }
    }
    
    internal class Handler : IRequestHandler<Request, GuildConfigEntity>
    {
        private readonly GuildContext _db;

        public Handler(GuildContext db) => _db = db;

        public async Task<GuildConfigEntity> Handle(Request request, CancellationToken cancellationToken)
        {
            var config = await _db
                              .GuildConfigs
                              .Include(g => g.Greetings)
                              .Include(c => c.Invites)
                              .Include(c => c.Invites.Whitelist)
                              .Include(c => c.InfractionSteps)
                              .Include(c => c.Exemptions)
                              .Include(c => c.Logging)
                              .Include(c => c.Logging.MemberJoins)
                              .Include(c => c.Logging.MemberLeaves)
                              .Include(c => c.Logging.MessageDeletes)
                              .Include(c => c.Logging.MessageEdits)
                              .Include(c => c.Logging.Infractions)
                              .AsSplitQuery()
                              .FirstAsync(c => c.GuildID == request.GuildID, cancellationToken);


            if (request.Greetings.IsDefined(out var greetings))
                config.Greetings = greetings;
            
            if (request.MuteRoleID.IsDefined(out Snowflake muteRole))
                config.MuteRoleID = muteRole;
            
            if (request.UseNativeMute.IsDefined(out bool useNativeMute))
                config.UseNativeMute = useNativeMute;

            if (request.EscalateInfractions.IsDefined(out bool escalate))
                config.ProgressiveStriking = escalate;

            if (request.MaxUserMentions.IsDefined(out int maxUserMentions))
                config.MaxUserMentions = maxUserMentions;

            if (request.MaxRoleMentions.IsDefined(out int maxRoleMentions))
                config.MaxRoleMentions = maxRoleMentions;

            if (request.UseAggressiveRegex.IsDefined(out bool useAggressiveRegex))
                config.Invites.UseAggressiveRegex = useAggressiveRegex;

            if (request.ScanInvites.IsDefined(out bool scanInvites))
                config.Invites.ScanOrigin = scanInvites;

            if (request.DeletePhishingLinks.IsDefined(out bool deletePhishingLinks))
                config.DeletePhishingLinks = deletePhishingLinks;

            if (request.DetectPhishingLinks.IsDefined(out bool detectPhishingLinks))
                config.DetectPhishingLinks = detectPhishingLinks;

            if (request.BlacklistInvites.IsDefined(out bool blacklistInvites))
                config.Invites.WhitelistEnabled = blacklistInvites;

            if (request.WarnOnMatchedInvite.IsDefined(out bool warnOnMatchedInvite))
                config.Invites.WarnOnMatch = warnOnMatchedInvite;

            if (request.DeleteOnMatchedInvite.IsDefined(out bool deleteOnMatchedInvite))
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
            
            if (request.Exemptions.IsDefined(out List<ExemptionEntity>? exemptions))
            {
                _db.RemoveRange(config.Exemptions.Except(exemptions));
                config.Exemptions = exemptions;
            }

            config.NamedInfractionSteps = request.NamedInfractionSteps ?? config.NamedInfractionSteps;

            if (request.InfractionSteps.IsDefined(out List<InfractionStepEntity>? infractionSteps))
            {
                _db.RemoveRange(config.InfractionSteps.Except(infractionSteps));
                config.InfractionSteps = infractionSteps;
            }

            if (request.AllowedInvites.IsDefined(out List<InviteEntity>? whitelistedInvites))
            {
                _db.RemoveRange(config.Invites.Whitelist.Except(whitelistedInvites));
                config.Invites.Whitelist = whitelistedInvites;
            }
            
            await _db.SaveChangesAsync(cancellationToken);
            
            return config;
        }
    }
}