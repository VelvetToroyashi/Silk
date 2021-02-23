using System.Collections.Generic;
using MediatR;
using Silk.Data.Models;

namespace Silk.Data.MediatR
{
    public class GuildConfigRequest
    {
        public class Get : IRequest<GuildConfig>
        {
            public ulong GuildId { get; init; }
        }

        public class Update : IRequest<GuildConfig?>
        {
            public ulong GuildId { get; init; } = default;

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
            public List<BlacklistedWord>? BlacklistedWords { get; init; }
        }
    }
}