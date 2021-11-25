using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Api.Data;
using Silk.Api.Data.Models;

namespace Silk.Api.Domain.Feature.Infractions
{
	public static class GetInfractionByGuild
	{
		public sealed record Request(ulong GuildId, [property: JsonIgnore] string AddedBy) : IRequest<IEnumerable<InfractionDto>>;

		public sealed record InfractionDto(Guid Key, InfractionType type, ulong Target, ulong Enforcer, ulong Guild, DateTime Created, DateTime? Expired, string Reason, bool Pardoned);
		
		public sealed class Handler : IRequestHandler<Request, IEnumerable<InfractionDto>>
		{
			private readonly ApiContext _db;
			public Handler(ApiContext db) => _db = db;
			
			public async Task<IEnumerable<InfractionDto>> Handle(Request request, CancellationToken cancellationToken)
			{
				var infractions = await _db.Infractions
					.Where(inf => inf.AddedByFK == request.AddedBy &&
					              inf.GuildCreationId == request.GuildId)
					.ToListAsync(cancellationToken);


				return infractions.Select(inf => new InfractionDto(inf.Key, inf.Type, inf.TargetUserId, inf.EnforcerUserId, inf.GuildCreationId, inf.Created, inf.Expires, inf.Reason, inf.IsPardoned));
			}
		}

		public sealed class Validator : AbstractValidator<Request>
		{
			public Validator()
			{
				RuleFor(g => g.GuildId)
					.NotEmpty();
			}
		}
	}
}