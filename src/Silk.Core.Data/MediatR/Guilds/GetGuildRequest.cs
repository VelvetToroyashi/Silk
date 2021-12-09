using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.MediatR.Guilds;

/// <summary>
///     Request for retrieving a <see cref="GuildEntity" />.
/// </summary>
/// <param name="GuildId">The Id of the Guild</param>
public record GetGuildRequest(ulong GuildId) : IRequest<GuildEntity>;

/// <summary>
///     The default handler for <see cref="GetGuildRequest" />.
/// </summary>
public class GetGuildHandler : IRequestHandler<GetGuildRequest, GuildEntity>
{
	private readonly GuildContext _db;
	public GetGuildHandler(GuildContext db) => _db = db;


	public async Task<GuildEntity> Handle(GetGuildRequest request, CancellationToken cancellationToken)
	{
		GuildEntity? guild = await _db.Guilds
			.AsSplitQuery()
			.AsNoTracking()
			.Include(g => g.Users)
			.Include(g => g.Infractions)
			.Include(g => g.Configuration)
			.FirstOrDefaultAsync(g => g.Id == request.GuildId, cancellationToken);

		return guild;
	}
}