using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
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
			public ulong GuildCreationId { get; set; }
		
			public DateTime Created { get; set; }
			public DateTime? Expires { get; set; }
		
			public string Reason { get; set; }
		}

		public sealed record ApiModel
		{
			public Guid  Key { get; set; }
			public ulong TargetUserId { get; set; }
			public ulong EnforcerUserId { get; set; }
			public ulong GuildCreationId { get; set; }
		
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
					GuildCreationId = request.GuildCreationId,
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
					GuildCreationId = entity.GuildCreationId,
					Created = entity.Created,
					Expires = entity.Expires,
					Reason = entity.Reason
				};
			}
		}

		public sealed class Validator : AbstractValidator<Request>
		{
			public Validator()
			{
				RuleFor(r => r.TargetUserId)
					.NotEmpty()
					.WithMessage("targetUserId: must not be empty.");

				RuleFor(r => r.EnforcerUserId)
					.NotEmpty()
					.WithMessage("enforcerUserId: must not be empty.");

				RuleFor(r => r.GuildCreationId)
					.NotEmpty()
					.WithMessage("guildCreationId: must nto be empty.");

				RuleFor(r => r.Created.ToUniversalTime())
					.LessThan(DateTime.UtcNow)
					.WithMessage("created: appears to be in the future.");

				RuleFor(r => r.Expires)
					.LessThan(DateTime.UtcNow)
					.Unless(r => r.Expires is null)
					.WithMessage("expires: appears to be in the past.");
				
				RuleFor(r => r.Reason)
					.MaximumLength(4000)
					.WithMessage("Reason: must not exceed 4000 characters.");
			}
		}
	}
}