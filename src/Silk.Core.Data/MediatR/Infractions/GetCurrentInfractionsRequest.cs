using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.DTOs;

namespace Silk.Core.Data.MediatR.Infractions
{
	public record GetCurrentInfractionsRequest : IRequest<IEnumerable<InfractionDTO>>;
	
	public class GetCurrentInfractionsHandler : IRequestHandler<GetCurrentInfractionsRequest, IEnumerable<InfractionDTO>>
	{
		private readonly GuildContext _db;
		public GetCurrentInfractionsHandler(GuildContext db) => _db = db;
		public async Task<IEnumerable<InfractionDTO>> Handle(GetCurrentInfractionsRequest request, CancellationToken cancellationToken)
		{
			var infractions = await _db.Infractions
				.Where(inf => !inf.Handled)
				.Where(inf => inf.HeldAgainstUser)
				.Where(inf => inf.Expiration.HasValue) // This is dangerous because it's not gauranteed to be of a correct type, but eh. //
				.Select(inf => new InfractionDTO(inf))
				.ToListAsync(cancellationToken);

			return infractions;
		}
	}
}