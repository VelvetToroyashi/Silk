using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.MediatR.Guilds
{
    public record UpdateGuildModConfigRequest : IRequest<GuildModConfigEntity?>
    {
        public UpdateGuildModConfigRequest(ulong guildId)
        {
            GuildId = guildId;
        }
        public ulong GuildId { get; init; }

        public bool? EscalateInfractions { get; init; }
        public bool? ScanInvites         { get; init; }
        public bool? BlacklistWords      { get; init; }
        public bool? BlacklistInvites    { get; init; }
        public bool? LogMembersJoining   { get; init; }
        public bool? LogMembersLeaving   { get; init; }
        public bool? UseAggressiveRegex  { get; init; }
        public bool? WarnOnMatchedInvite { get; init; }

        public bool? DetectPhishingLinks { get; init; }

        public bool? DeletePhishingLinks { get; init; }

        public bool? DeleteOnMatchedInvite { get; init; }
        public int?  MaxUserMentions       { get; init; }
        public int?  MaxRoleMentions       { get; init; }

        public List<InviteEntity>? AllowedInvites { get; init; }

        public List<ExemptionEntity>?      Exemptions        { get; init; }
        public List<InfractionStepEntity>? InfractionSteps   { get; init; }
        public ulong?                      MuteRoleId        { get; init; }
        public ulong?                      LoggingChannel    { get; init; }
        public bool?                       LogMessageChanges { get; init; }
        public bool?                       UseWebhookLogging { get; init; }
        public ulong?                      WebhookLoggingId  { get; init; }
        public string?                     WebhookLoggingUrl { get; init; }

        public Dictionary<string, InfractionStepEntity>? AutoModActions { get; init; }
    }

    public sealed class UpdateGuildModConfigHandler : IRequestHandler<UpdateGuildModConfigRequest, GuildModConfigEntity?>
    {
        private readonly GuildContext _db;
        public UpdateGuildModConfigHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<GuildModConfigEntity?> Handle(UpdateGuildModConfigRequest request, CancellationToken cancellationToken)
        {
            GuildModConfigEntity? config = await _db.GuildModConfigs
                                                    .AsNoTracking()
                                                    .Include(c => c.InfractionSteps)
                                                    .Include(c => c.AllowedInvites)
                                                    .FirstAsync(g => g.GuildId == request.GuildId, cancellationToken);

            config.MuteRoleId = request.MuteRoleId                              ?? config.MuteRoleId;
            config.AutoEscalateInfractions = request.EscalateInfractions        ?? config.AutoEscalateInfractions;
            config.LogMessageChanges = request.LogMessageChanges                ?? config.LogMessageChanges;
            config.MaxUserMentions = request.MaxUserMentions                    ?? config.MaxUserMentions;
            config.MaxRoleMentions = request.MaxRoleMentions                    ?? config.MaxRoleMentions;
            config.LoggingChannel = request.LoggingChannel                      ?? config.LoggingChannel;
            config.ScanInvites = request.ScanInvites                            ?? config.ScanInvites;
            config.DetectPhishingLinks = request.DetectPhishingLinks            ?? config.DetectPhishingLinks;
            config.DeletePhishingLinks = request.DeletePhishingLinks            ?? config.DeletePhishingLinks;
            config.BlacklistWords = request.BlacklistWords                      ?? config.BlacklistWords;
            config.BlacklistInvites = request.BlacklistInvites                  ?? config.BlacklistInvites;
            config.LogMemberJoins = request.LogMembersJoining                   ?? config.LogMemberJoins;
            config.LogMemberLeaves = request.LogMembersLeaving                  ?? config.LogMemberLeaves;
            config.UseAggressiveRegex = request.UseAggressiveRegex              ?? config.UseAggressiveRegex;
            config.WarnOnMatchedInvite = request.WarnOnMatchedInvite            ?? config.WarnOnMatchedInvite;
            config.UseWebhookLogging = request.UseWebhookLogging                ?? config.UseWebhookLogging;
            config.WebhookLoggingId = request.WebhookLoggingId                  ?? config.WebhookLoggingId;
            config.LoggingWebhookUrl = request.WebhookLoggingUrl                ?? config.LoggingWebhookUrl;
            config.DeleteMessageOnMatchedInvite = request.DeleteOnMatchedInvite ?? config.DeleteMessageOnMatchedInvite;
            config.Exemptions = request.Exemptions                              ?? config.Exemptions;
            config.NamedInfractionSteps = request.AutoModActions                ?? config.NamedInfractionSteps;

            if (request.InfractionSteps?.Any() ?? false)
            {
                _db.RemoveRange(config.InfractionSteps.Except(request.InfractionSteps!));
                config.InfractionSteps = request.InfractionSteps!;
            }

            if (request.AllowedInvites?.Any() ?? false)
            {
                _db.RemoveRange(config.AllowedInvites.Except(request.AllowedInvites!));
                config.AllowedInvites = request.AllowedInvites!;
            }

            EntityEntry<GuildModConfigEntity>? updatedEntry = _db.GuildModConfigs.Update(config);
            await _db.SaveChangesAsync(cancellationToken);

            return updatedEntry.Entity;
        }
    }
}