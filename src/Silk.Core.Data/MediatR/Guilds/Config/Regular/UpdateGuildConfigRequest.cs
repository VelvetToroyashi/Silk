using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Guilds
{
    /// <summary>
    ///     Request for updating a <see cref="GuildConfig" /> for a Guild.
    /// </summary>
    /// <param name="GuildId">The Id of the Guild</param>
    public record UpdateGuildConfigRequest(ulong GuildId) : IRequest<GuildConfig?>
    {
        

        public ulong? GreetingChannelId { get; init; }
        public ulong? VerificationRoleId { get; init; }

        
        public GreetingOption? GreetingOption { get; init; }
        

        
        
        public bool? GreetOnScreeningComplete { get; init; }

        

        public string? GreetingText { get; init; }

        
        public List<DisabledCommand>? DisabledCommands { get; init; }

        

        //public List<BlacklistedWord>? BlacklistedWords { get; init; }
    }

    /// <summary>
    ///     The default handler for <see cref="UpdateGuildConfigRequest" />.
    /// </summary>
    public class UpdateGuildConfigHandler : IRequestHandler<UpdateGuildConfigRequest, GuildConfig?>
    {
        private readonly GuildContext _db;

        public UpdateGuildConfigHandler(GuildContext db) => _db = db;

        public async Task<GuildConfig?> Handle(UpdateGuildConfigRequest request, CancellationToken cancellationToken)
        {
            GuildConfig config = await _db.GuildConfigs
                .AsSplitQuery()
                .FirstOrDefaultAsync(g => g.GuildId == request.GuildId, cancellationToken);

            
            config.GreetingOption = request.GreetingOption ?? config.GreetingOption;
            
            config.GreetingChannel = request.GreetingChannelId ?? config.GreetingChannel;
            config.VerificationRole = request.VerificationRoleId ?? config.VerificationRole;

            config.GreetingText = request.GreetingText ?? config.GreetingText;
            config.DisabledCommands = request.DisabledCommands ?? config.DisabledCommands;
            
            await _db.SaveChangesAsync(cancellationToken);
            return config;
        }
    }
}