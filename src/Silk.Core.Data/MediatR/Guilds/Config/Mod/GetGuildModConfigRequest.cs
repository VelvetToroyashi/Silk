using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Guilds.Config
{
	public sealed record GetGuildModConfigRequest(ulong guildId) : IRequest<GuildModConfig>;
	
	public sealed class GetGuildModConfigHandler : IRequestHandler<GetGuildModConfigRequest, GuildModConfig>
	{
		private readonly GuildContext _db;
		public GetGuildModConfigHandler(GuildContext db) => _db = db;

		public Task<GuildModConfig> Handle(GetGuildModConfigRequest request, CancellationToken cancellationToken)
			=> _db.GuildModConfigs
				.Include(c => c.AllowedInvites)
				.Include(c => c.InfractionSteps)
				.FirstOrDefaultAsync(c => c.GuildId == request.guildId, cancellationToken);
	}
}