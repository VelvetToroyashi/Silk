using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
			var user = await _context
				.Users
				.Include(u => u.Infractions)
				.FirstOrDefaultAsync(u => u.Id == request.UserId && u.GuildId == request.GuildId, cancellationToken);
			
			return user?.Infractions.Select(inf => new InfractionDTO(inf)) ?? Array.Empty<InfractionDTO>();
		}
	}
}