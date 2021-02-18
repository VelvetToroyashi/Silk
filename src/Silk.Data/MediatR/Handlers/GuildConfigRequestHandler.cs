using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Data.Models;

namespace Silk.Data.MediatR.Handlers
{
    public class GuildConfigRequestHandler
    {
        public class GuildConfigGetRequestHandler : IRequestHandler<GuildConfigRequest.GetGuildConfigRequest, GuildConfig?>
        {
            private readonly SilkDbContext _db;


            public GuildConfigGetRequestHandler(SilkDbContext db)
            {
                _db = db;
            }
            
            public async Task<GuildConfig?> Handle(GuildConfigRequest.GetGuildConfigRequest request, CancellationToken cancellationToken)
            {
                GuildConfig config = await _db.GuildConfigs.FirstOrDefaultAsync(g => g.GuildId == request.GuildId, cancellationToken);
                return config;
            }
        }

        public class GuildConfigUpdateRequestHandler : IRequestHandler<GuildConfigRequest.UpdateGuildConfigRequest, GuildConfig?>
        {
            private readonly SilkDbContext _db;

            public GuildConfigUpdateRequestHandler(SilkDbContext db)
            {
                _db = db;
            }
            
            public async Task<GuildConfig?> Handle(GuildConfigRequest.UpdateGuildConfigRequest request, CancellationToken cancellationToken)
            {
                GuildConfig config = await _db.GuildConfigs.FirstOrDefaultAsync(g => g.GuildId == request.GuildId, cancellationToken);

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

                config.AllowedInvites = request.AllowedInvites ?? config.AllowedInvites;
                config.BlackListedWords = request.BlacklistedWords ?? config.BlackListedWords;

                await _db.SaveChangesAsync(cancellationToken);
                return config;
            }
        }
    }
}