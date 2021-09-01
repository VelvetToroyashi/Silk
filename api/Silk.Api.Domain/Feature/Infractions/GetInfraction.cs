using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Api.Data;
using Silk.Api.Data.Models;

namespace Silk.Api.Domain.Feature.Infractions
{
	public static class GetInfraction
	{
		public sealed record Request(Guid Key) : IRequest<ApiModel>;

		public sealed record ApiModel
		{
			public InfractionType Type { get; init; }
			
			public ulong TargetUserId { get; init; }
			public ulong EnforcerUserId { get; init; }
			public ulong GuildCreationId { get; init; }
		
			public DateTime Created { get; init; }
			public DateTime? Expires { get; init; }
			
			public bool IsPardoned { get; init; }
		
			public string Reason { get; init; }
		}

		public class Handler : IRequestHandler<Request, ApiModel>
		{
			private readonly ApiContext _db;
			public Handler(ApiContext db) => _db = db;

			public async Task<ApiModel> Handle(Request request, CancellationToken cancellationToken)
			{
				var infraction = await _db
					.Infractions
					.FirstOrDefaultAsync(inf => inf.Key == request.Key, cancellationToken);
				
				return ToModel(infraction);
			}
		}
		
		private static ApiModel ToModel(InfractionEntity entity)
		{
			if (entity is null) return null;
				
			return new()
			{
				Type = entity.Type,
				TargetUserId = entity.TargetUserId,
				EnforcerUserId = entity.EnforcerUserId,
				GuildCreationId = entity.GuildCreationId,
				Created = entity.Created,
				Expires = entity.Expires,
				IsPardoned = entity.IsPardoned,
				Reason = entity.Reason
			};
		}
	}
}