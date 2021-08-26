using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Silk.Api.Data;
using Silk.Api.Data.Models;

namespace Silk.Api.Domain.Feature.Infractions
{
	public static class AddInfraction
	{
		public sealed record Request : IRequest<ApiModel>
		{
			public ulong TargetUserId { get; set; }
			public ulong EnforcerUserId { get; set; }
			public ulong GuilldCreationId { get; set; }
		
			public DateTime Created { get; set; }
			public DateTime? Expires { get; set; }
		
			public string Reason { get; set; }
		}

		public sealed record ApiModel
		{
			public Guid  Key { get; set; }
			public ulong TargetUserId { get; set; }
			public ulong EnforcerUserId { get; set; }
			public ulong GuilldCreationId { get; set; }
		
			public DateTime Created { get; set; }
			public DateTime? Expires { get; set; }
		
			public string Reason { get; set; }
		}
		
		public sealed class Handler : IRequestHandler<Request, ApiModel>
		{
			private readonly ApiContext _db;
			public Handler(ApiContext db) => _db = db;

			public async Task<ApiModel> Handle(Request request, CancellationToken cancellationToken)
			{
				var entity = new Infraction()
				{
					Key = Guid.NewGuid(),
					TargetUserId = request.TargetUserId,
					EnforcerUserId = request.EnforcerUserId,
					GuilldCreationId = request.GuilldCreationId,
					Created = request.Created,
					Expires = request.Expires,
					Reason = request.Reason
				};

				_db.Add(entity);
				await _db.SaveChangesAsync(cancellationToken);

				return ToModel(entity);
			}

			private static ApiModel ToModel(Infraction entity)
			{
				return new()
				{
					Key = entity.Key,
					TargetUserId = entity.TargetUserId,
					EnforcerUserId = entity.EnforcerUserId,
					GuilldCreationId = entity.GuilldCreationId,
					Created = entity.Created,
					Expires = entity.Expires,
					Reason = entity.Reason
				};
			}
		}
	}
}