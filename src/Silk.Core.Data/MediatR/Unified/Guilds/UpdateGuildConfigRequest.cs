using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.Guilds
{
    /// <summary>
    ///     Request for updating a <see cref="GuildConfig" /> for a Guild.
    /// </summary>
    /// <param name="GuildId">The Id of the Guild</param>
    public record UpdateGuildConfigRequest(ulong GuildId) : IRequest<GuildConfig?>
    {
        public ulong? MuteRoleId { get; init; }
        public ulong? LoggingChannel { get; init; }
        public ulong? GreetingChannelId { get; init; }
        public ulong? VerificationRoleId { get; init; }

        public bool? ScanInvites { get; init; }
        public bool? GreetMembers { get; init; }
        public bool? BlacklistWords { get; init; }
        public bool? BlacklistInvites { get; init; }
        public bool? LogMembersJoining { get; init; }
        public bool? UseAggressiveRegex { get; init; }
        public bool? WarnOnMatchedInvite { get; init; }
        public bool? DeleteOnMatchedInvite { get; init; }
        public bool? GreetOnVerificationRole { get; init; }
        public bool? GreetOnScreeningComplete { get; init; }

        public int? MaxUserMentions { get; init; }
        public int? MaxRoleMentions { get; init; }

        public string? GreetingText { get; init; }

        public List<Invite>? AllowedInvites { get; init; }
        public List<DisabledCommand>? DisabledCommands { get; init; }
        public List<SelfAssignableRole>? SelfAssignableRoles { get; init; }
        public List<InfractionStep>? InfractionSteps { get; init; }
        //public List<BlacklistedWord>? BlacklistedWords { get; init; }
    }

    /// <summary>
    ///     The default handler for <see cref="UpdateGuildConfigRequest" />.
    /// </summary>
    public class UpdateGuildConfigHandler : IRequestHandler<UpdateGuildConfigRequest, GuildConfig?>
    {
        private readonly GuildContext _db;

        public UpdateGuildConfigHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<GuildConfig?> Handle(UpdateGuildConfigRequest request, CancellationToken cancellationToken)
        {
            GuildConfig config = await _db.GuildConfigs
                .Include(c => c.SelfAssignableRoles)
                .AsSplitQuery()
                .FirstOrDefaultAsync(g => g.GuildId == request.GuildId, cancellationToken);

            config.MuteRoleId = request.MuteRoleId ?? config.MuteRoleId;
            config.GreetMembers = request.GreetMembers ?? config.GreetMembers;
            config.LoggingChannel = request.LoggingChannel ?? config.LoggingChannel;
            config.GreetingChannel = request.GreetingChannelId ?? config.GreetingChannel;
            config.VerificationRole = request.VerificationRoleId ?? config.VerificationRole;

            config.ScanInvites = request.ScanInvites ?? config.ScanInvites;
            config.BlacklistWords = request.BlacklistWords ?? config.BlacklistWords;
            config.BlacklistInvites = request.BlacklistInvites ?? config.BlacklistInvites;
            config.LogMemberJoing = request.LogMembersJoining ?? config.LogMemberJoing;
            config.UseAggressiveRegex = request.UseAggressiveRegex ?? config.UseAggressiveRegex;
            config.WarnOnMatchedInvite = request.WarnOnMatchedInvite ?? config.WarnOnMatchedInvite;
            config.DeleteMessageOnMatchedInvite = request.DeleteOnMatchedInvite ?? config.DeleteMessageOnMatchedInvite;
            config.GreetOnVerificationRole = request.GreetOnVerificationRole ?? config.GreetOnVerificationRole;
            config.GreetOnScreeningComplete = request.GreetOnScreeningComplete ?? config.GreetOnVerificationRole;

            config.MaxUserMentions = request.MaxUserMentions ?? config.MaxUserMentions;
            config.MaxRoleMentions = request.MaxRoleMentions ?? config.MaxRoleMentions;

            config.GreetingText = request.GreetingText ?? config.GreetingText;

            config.InfractionSteps = request.InfractionSteps ?? config.InfractionSteps;
            config.AllowedInvites = request.AllowedInvites ?? config.AllowedInvites;
            config.DisabledCommands = request.DisabledCommands ?? config.DisabledCommands;

            if (request.SelfAssignableRoles is not null)
            {
                foreach (var r in request.SelfAssignableRoles)
                {
                    if (config.SelfAssignableRoles.Any(ro => ro.Id == r.Id))
                        config.SelfAssignableRoles.Remove(r);
                    else config.SelfAssignableRoles.Add(r);
                }
            }

            await _db.SaveChangesAsync(cancellationToken);
            return config;
        }
    }
}