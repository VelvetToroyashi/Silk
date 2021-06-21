using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.DTOs;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Infractions
{
	public sealed record CreateInfractionRequest(ulong User, ulong Enforcer, ulong Guild, string Reason, InfractionType Type, DateTime? Expiration, bool HeldAgainstUser = true) : IRequest<InfractionDTO>;
	
	public class CreateInfractionHandler : IRequestHandler<CreateInfractionRequest, InfractionDTO>
	{
		private readonly GuildContext _db;
		public CreateInfractionHandler(GuildContext db) => _db = db;

		public async Task<InfractionDTO> Handle(CreateInfractionRequest request, CancellationToken cancellationToken)
		{
			var guild = await _db.Guilds
				.Include(g => g.Infractions)
				.FirstAsync(g => g.Id == request.Guild, cancellationToken);

			var infraction = new Infraction
			{
				GuildId = request.Guild,
				Enforcer = request.Enforcer,
				Reason = request.Reason,
				HeldAgainstUser = request.HeldAgainstUser,
				Expiration =  request.Expiration,
				InfractionTime = DateTime.UtcNow,
				UserId = request.User,
				InfractionType = request.Type
			};
			
			guild.Infractions.Add(infraction);

			await _db.SaveChangesAsync(cancellationToken);

			return new(infraction);
		}
	}
}