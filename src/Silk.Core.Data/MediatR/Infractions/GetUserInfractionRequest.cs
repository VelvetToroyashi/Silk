using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.MediatR.Infractions
{
	public sealed record GetUserInfractionRequest(
		ulong UserId, ulong GuildId, InfractionType Type,
		int? CaseId = null) : IRequest<InfractionEntity?>;

	public sealed class GetUserInfractionHandler : IRequestHandler<GetUserInfractionRequest, InfractionEntity?>
	{
		private readonly GuildContext _db;
		public GetUserInfractionHandler(GuildContext db)
		{
			_db = db;
		}

		public async Task<InfractionEntity?> Handle(GetUserInfractionRequest request, CancellationToken cancellationToken)
		{
			InfractionEntity? inf;
			if (request.CaseId is not null)
			{
				inf = await _db.Infractions
					.Where(inf => inf.CaseNumber == request.CaseId)
					.SingleOrDefaultAsync(cancellationToken);
			}
			else
			{
				inf = await _db.Infractions
					.Where(inf => inf.UserId == request.UserId)
					.Where(inf => inf.GuildId == request.GuildId)
					.Where(inf => inf.InfractionType == request.Type)
					.Where(inf => !inf.HeldAgainstUser)
					.OrderBy(inf => inf.CaseNumber)
					.LastOrDefaultAsync(cancellationToken);
			}

			return inf;
		}
	}
}