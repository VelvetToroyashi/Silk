using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.DTOs;
using Silk.Core.Data.MediatR.Users;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Infractions
{
	public sealed record CreateInfractionRequest(ulong User, ulong Enforcer, ulong Guild, string Reason, InfractionType Type, DateTime? Expiration, bool HeldAgainstUser = true) : IRequest<InfractionDTO>;
	
	public class CreateInfractionHandler : IRequestHandler<CreateInfractionRequest, InfractionDTO>
	{
		private readonly GuildContext _db;
		private readonly IMediator _mediator;
		public CreateInfractionHandler(GuildContext db, IMediator mediator)
		{
			_db = db;
			_mediator = mediator;
		}

		public async Task<InfractionDTO> Handle(CreateInfractionRequest request, CancellationToken cancellationToken)
		{
			var guildInfracionCount = await _db.Infractions
				.Where(inf => inf.InfractionType != InfractionType.Note)
				.Where(inf => inf.GuildId == request.Guild)				
				.CountAsync(cancellationToken) + 1;

			var infraction = new Infraction
			{
				GuildId = request.Guild,
				CaseNumber = guildInfracionCount,
				Enforcer = request.Enforcer,
				Reason = request.Reason,
				HeldAgainstUser = request.HeldAgainstUser,
				Expiration =  request.Expiration,
				InfractionTime = DateTime.UtcNow,
				UserId = request.User,
				InfractionType = request.Type
			};

			_db.Infractions.Add(infraction);
			await _mediator.Send(new GetOrCreateUserRequest(request.Guild, request.User), cancellationToken);
			
			await _db.SaveChangesAsync(cancellationToken);

			return new(infraction);
		}
	}
}