using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Silk.Core.Data.DTOs;

namespace Silk.Core.Data.MediatR.Infractions
{
	public record GetUserInfractionsRequest(ulong GuildId, ulong UserId) : IRequest<IEnumerable<InfractionDTO>>;

	public class GetUserInfractionsHandler : IRequestHandler<GetUserInfractionsRequest, IEnumerable<InfractionDTO>>
	{
		private readonly GuildContext _context;
		public GetUserInfractionsHandler(GuildContext context) => _context = context;
		
		public async Task<IEnumerable<InfractionDTO>> Handle(GetUserInfractionsRequest request, CancellationToken cancellationToken)
		{

			return Array.Empty<InfractionDTO>();
		}
	}
}