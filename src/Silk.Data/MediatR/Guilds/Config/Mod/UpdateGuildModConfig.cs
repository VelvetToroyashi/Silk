using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Remora.Rest.Core;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Guilds;

public static class UpdateGuildModConfig
{
    public sealed record Request(Snowflake GuildID) : IRequest<GuildModConfigEntity?>
    {
        public Optional<bool>      ScanInvites           { get; init; }
        public Optional<Snowflake> MuteRoleID            { get; init; }
        public Optional<bool>      UseNativeMute        { get; init; }
        public Optional<int>       MaxUserMentions       { get; init; }
        public Optional<int>       MaxRoleMentions       { get; init; }
        public Optional<bool>      BlacklistInvites      { get; init; }
        public Optional<bool>      LogMembersJoining     { get; init; }
        public Optional<bool>      LogMembersLeaving     { get; init; }
        public Optional<bool>      UseAggressiveRegex    { get; init; }
        public Optional<bool>      EscalateInfractions   { get; init; }
        public Optional<bool>      WarnOnMatchedInvite   { get; init; }
        public Optional<bool>      DetectPhishingLinks   { get; init; }
        public Optional<bool>      DeletePhishingLinks   { get; init; }
        public Optional<bool>      DeleteOnMatchedInvite { get; init; }

        public Optional<GuildLoggingConfigEntity> LoggingConfig { get; init; }

        public Optional<List<InviteEntity>>              AllowedInvites       { get; init; }
        public Optional<List<ExemptionEntity>>           Exemptions           { get; init; }
        public Optional<List<InfractionStepEntity>>      InfractionSteps      { get; init; }
        public Dictionary<string, InfractionStepEntity>? NamedInfractionSteps { get; init; }
    }

    internal sealed class Handler : IRequestHandler<Request, GuildModConfigEntity?>
    {
        private readonly GuildContext _db;
        public Handler(GuildContext db) => _db = db;

        public async Task<GuildModConfigEntity?> Handle(Request request, CancellationToken cancellationToken)
        {
            GuildModConfigEntity config = await _db.GuildModConfigs
                                                   .AsNoTracking()
                                                   .Include(c => c.InfractionSteps)
                                                   .Include(c => c.AllowedInvites)
                                                   .FirstAsync(g => g.GuildID == request.GuildID, cancellationToken);


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
                config.UseAggressiveRegex = useAggressiveRegex;

            if (request.ScanInvites.IsDefined(out bool scanInvites))
                config.ScanInviteOrigin = scanInvites;

            if (request.DeletePhishingLinks.IsDefined(out bool deletePhishingLinks))
                config.DeletePhishingLinks = deletePhishingLinks;

            if (request.DetectPhishingLinks.IsDefined(out bool detectPhishingLinks))
                config.DetectPhishingLinks = detectPhishingLinks;

            if (request.BlacklistInvites.IsDefined(out bool blacklistInvites))
                config.WhitelistInvites = blacklistInvites;

            if (request.WarnOnMatchedInvite.IsDefined(out bool warnOnMatchedInvite))
                config.InfractOnMatchedInvite = warnOnMatchedInvite;

            if (request.DeleteOnMatchedInvite.IsDefined(out bool deleteOnMatchedInvite))
                config.DeleteMessageOnMatchedInvite = deleteOnMatchedInvite;

            if (request.LogMembersJoining.IsDefined(out bool logMembersJoining))
                config.LoggingConfig.LogMemberJoins = logMembersJoining;

            if (request.LogMembersLeaving.IsDefined(out bool logMembersLeaving))
                config.LoggingConfig.LogMemberLeaves = logMembersLeaving;

            if (request.Exemptions.IsDefined(out List<ExemptionEntity>? exemptions))
                config.Exemptions = exemptions;

            config.NamedInfractionSteps = request.NamedInfractionSteps ?? config.NamedInfractionSteps;

            if (request.InfractionSteps.IsDefined(out List<InfractionStepEntity>? infractionSteps))
            {
                _db.RemoveRange(config.InfractionSteps.Except(infractionSteps));
                config.InfractionSteps = infractionSteps;
            }

            if (request.AllowedInvites.IsDefined(out List<InviteEntity>? whitelistedInvites))
            {
                _db.RemoveRange(config.AllowedInvites.Except(whitelistedInvites));
                config.AllowedInvites = whitelistedInvites;
            }

            _db.Update(config);
            
            await _db.SaveChangesAsync(cancellationToken);
            return config;
        }
    }
}