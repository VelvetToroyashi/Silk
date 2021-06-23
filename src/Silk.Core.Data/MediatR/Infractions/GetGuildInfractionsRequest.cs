using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.DTOs;

namespace Silk.Core.Data.MediatR.Infractions
{
	public sealed record GetGuildInfractionsRequest(ulong GuildId) : IRequest<IEnumerable<InfractionDTO>>;

	public sealed class GetGuildInfractionHandler : IRequestHandler<GetGuildInfractionsRequest, IEnumerable<InfractionDTO>>
	{
		private readonly GuildContext _db;
		public GetGuildInfractionHandler(GuildContext db) => _db = db;

		public async Task<IEnumerable<InfractionDTO>> Handle(GetGuildInfractionsRequest request, CancellationToken cancellationToken)
		{
			var infractions = await 
				_db
				.Infractions
				.Where(inf => inf.GuildId == request.GuildId)
				.ToListAsync(cancellationToken);
			
			return infractions.Select(inf => new InfractionDTO(inf));
		}
	}
}