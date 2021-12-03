using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Remora.Rest.Core;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.MediatR.Guilds
{
    public record UpdateGuildModConfigRequest(ulong GuildId) : IRequest<GuildModConfigEntity?>
    {
        public Optional<bool>   ScanInvites           { get; init; }
        public Optional<ulong>  MuteRoleId            { get; init; }
        public Optional<ulong>  LoggingChannel        { get; init; }
        public Optional<int>    MaxUserMentions       { get; init; }
        public Optional<int>    MaxRoleMentions       { get; init; }
        public Optional<bool>   BlacklistInvites      { get; init; }
        public Optional<bool>   LogMembersJoining     { get; init; }
        public Optional<bool>   LogMembersLeaving     { get; init; }
        public Optional<bool>   LogMessageChanges     { get; init; }
        public Optional<bool>   UseWebhookLogging     { get; init; }
        public Optional<ulong>  WebhookLoggingId      { get; init; }
        public Optional<bool>   UseAggressiveRegex    { get; init; }
        public Optional<bool>   EscalateInfractions   { get; init; }
        public Optional<bool>   WarnOnMatchedInvite   { get; init; }
        public Optional<bool>   DetectPhishingLinks   { get; init; }
        public Optional<bool>   DeletePhishingLinks   { get; init; }
        public Optional<string> WebhookLoggingUrl     { get; init; }
        public Optional<bool>   DeleteOnMatchedInvite { get; init; }
        
        public Optional<GuildLoggingConfigEntity>        LoggingConfig   { get; init; }
        
        public Optional<List<InviteEntity>>              AllowedInvites  { get; init; }
        public Optional<List<ExemptionEntity>>           Exemptions      { get; init; }
        public Optional<List<InfractionStepEntity>>      InfractionSteps { get; init; }
        public Dictionary<string, InfractionStepEntity>? AutoModActions  { get; init; }
    }

    public sealed class UpdateGuildModConfigHandler : IRequestHandler<UpdateGuildModConfigRequest, GuildModConfigEntity?>
    {
        private readonly GuildContext _db;
        public UpdateGuildModConfigHandler(GuildContext db) => _db = db;

        public async Task<GuildModConfigEntity?> Handle(UpdateGuildModConfigRequest request, CancellationToken cancellationToken)
        {
            GuildModConfigEntity? config = await _db.GuildModConfigs
                                                    .AsNoTracking()
                                                    .Include(c => c.InfractionSteps)
                                                    .Include(c => c.AllowedInvites)
                                                    .FirstAsync(g => g.GuildId == request.GuildId, cancellationToken);

            
            if (request.MuteRoleId.IsDefined(out var muteRole))
                config.MuteRoleId =  muteRole;
            
            if (request.EscalateInfractions.IsDefined(out var escalate))
                config.AutoEscalateInfractions = escalate;
            
            if (request.LogMessageChanges.IsDefined(out var messageChanges))
                config.LogMessageChanges = messageChanges;
            
            if (request.MaxUserMentions.IsDefined(out var maxUserMentions))
                config.MaxUserMentions = maxUserMentions;
            
            if (request.MaxRoleMentions.IsDefined(out var maxRoleMentions))
                config.MaxRoleMentions = maxRoleMentions;
            
            if (request.UseAggressiveRegex.IsDefined(out var useAggressiveRegex))
                config.UseAggressiveRegex = useAggressiveRegex;
            
            if (request.ScanInvites.IsDefined(out var scanInvites))
                config.ScanInvites = scanInvites;
            
            if (request.DeletePhishingLinks.IsDefined(out var deletePhishingLinks))
                config.DeletePhishingLinks = deletePhishingLinks;
            
            if (request.DetectPhishingLinks.IsDefined(out var detectPhishingLinks))
                config.DetectPhishingLinks = detectPhishingLinks;

            if (request.BlacklistInvites.IsDefined(out var blacklistInvites))
                config.BlacklistInvites = blacklistInvites;
            
            if (request.WarnOnMatchedInvite.IsDefined(out var warnOnMatchedInvite))
                config.WarnOnMatchedInvite = warnOnMatchedInvite;
            
            if (request.DeleteOnMatchedInvite.IsDefined(out var deleteOnMatchedInvite))
                config.DeleteMessageOnMatchedInvite = deleteOnMatchedInvite;
            
            if (request.LogMembersJoining.IsDefined(out var logMembersJoining))
                config.LogMemberJoins = logMembersJoining;
            
            if (request.LogMembersLeaving.IsDefined(out var logMembersLeaving))
                config.LogMemberLeaves = logMembersLeaving;


            if (request.Exemptions.IsDefined(out var exemptions))
                config.Exemptions = exemptions;
            
            config.NamedInfractionSteps = request.AutoModActions                ?? config.NamedInfractionSteps;

            if (request.InfractionSteps.IsDefined(out var infractionSteps))
            {
                _db.RemoveRange(config.InfractionSteps.Except(infractionSteps));
                config.InfractionSteps = infractionSteps;
            }

            if (request.AllowedInvites.IsDefined(out var whitelistedInvites))
            {
                _db.RemoveRange(config.AllowedInvites.Except(whitelistedInvites));
                config.AllowedInvites = whitelistedInvites;
            }

            EntityEntry<GuildModConfigEntity>? updatedEntry = _db.GuildModConfigs.Update(config);
            await _db.SaveChangesAsync(cancellationToken);

            return updatedEntry.Entity;
        }
    }
}