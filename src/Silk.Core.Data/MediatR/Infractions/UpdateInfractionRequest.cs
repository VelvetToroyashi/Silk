using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.DTOs;

namespace Silk.Core.Data.MediatR.Infractions
{
	public sealed record UpdateInfractionRequest(int InfractionId, DateTime? Expiration, string? Reason = null, bool Rescinded = false, bool WasEscalated = false) : IRequest<InfractionDTO>;
	
	public sealed class UpdateInfractionHandler : IRequestHandler<UpdateInfractionRequest, InfractionDTO>
	{
		private readonly GuildContext _db;
		public UpdateInfractionHandler(GuildContext db) => _db = db;
		public async Task<InfractionDTO> Handle(UpdateInfractionRequest request, CancellationToken cancellationToken)
		{
			var infraction = await _db.Infractions.FirstAsync(inf => inf.Id == request.InfractionId, cancellationToken);

			infraction.Expiration = request.Expiration;
			infraction.Reason = request.Reason ?? infraction.Reason;
			infraction.HeldAgainstUser = !request.Rescinded;
			infraction.EscalatedFromStrike = request.WasEscalated;
			await _db.SaveChangesAsync(cancellationToken);
			return new(infraction);
		}
	}
}