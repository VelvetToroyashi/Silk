using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Handlers
{
    public class GuildConfigRequestHandler
    {
        public class GetHandler : IRequestHandler<GuildConfigRequest.Get, GuildConfig>
        {
            private readonly SilkDbContext _db;


            public GetHandler(SilkDbContext db)
            {
                _db = db;
            }
            
            public async Task<GuildConfig> Handle(GuildConfigRequest.Get request, CancellationToken cancellationToken)
            {
                GuildConfig config = 
                    await _db.GuildConfigs
                        .Include(c => c.DisabledCommands)
                        //.Include(c => c.BlackListedWords)
                        .Include(c => c.SelfAssignableRoles)
                        .AsSplitQuery()
                        .FirstOrDefaultAsync(g => g.GuildId == request.GuildId, cancellationToken);
                return config;
            }
        }

        public class UpdateHandler : IRequestHandler<GuildConfigRequest.Update, GuildConfig?>
        {
            private readonly SilkDbContext _db;

            public UpdateHandler(SilkDbContext db)
            {
                _db = db;
            }
            
            public async Task<GuildConfig?> Handle(GuildConfigRequest.Update request, CancellationToken cancellationToken)
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
                
                config.AllowedInvites = request.AllowedInvites ?? config.AllowedInvites;
                config.DisabledCommands = request.DisabledCommands ?? config.DisabledCommands;
                if (request.SelfAssignableRoles is not null)
                {
                    foreach (var r in request.SelfAssignableRoles)
                    {
                        if ( config.SelfAssignableRoles.Any(ro => ro.Id == r.Id))
                             config.SelfAssignableRoles.Remove(r);
                        else config.SelfAssignableRoles.Add(r);
                    }   
                }

                await _db.SaveChangesAsync(cancellationToken);
                return config;
            }
        }
    }
}