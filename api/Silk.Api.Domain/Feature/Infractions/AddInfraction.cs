using System;
using System.Text.Json.Serialization;
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
			[JsonPropertyName("target")]
			public ulong TargetUserId { get; init; }
			
			[JsonPropertyName("enforcer")]
			public ulong EnforcerUserId { get; init; }
			
			[JsonPropertyName("guild")]
			public ulong GuildCreationId { get; init; }
			
			[JsonPropertyName("type")]
			public InfractionType Type { get; init; }
			
			[JsonPropertyName("created")]
			public DateTime Created { get; init; }
			
			[JsonPropertyName("expiration")]
			public DateTime? Expires { get; init; }
		
			[JsonPropertyName("reason")]
			public string Reason { get; init; }
			
			[JsonIgnore]
			public string AddedBy { get; init; }
		}

		public sealed record ApiModel
		{
			[JsonPropertyName("key")]
			public Guid Key { get; init; }

			[JsonPropertyName("type")]
			public InfractionType Type { get; init; }
			
			[JsonPropertyName("target")]
			public ulong TargetUserId { get; init; }
			
			[JsonPropertyName("enforcer")]
			public ulong EnforcerUserId { get; init; }
			
			[JsonPropertyName("guild")]
			public ulong GuildCreationId { get; init; }
		
			[JsonPropertyName("created")]
			public DateTime Created { get; init; }
			
			[JsonPropertyName("expiration")]
			public DateTime? Expires { get; init; }
			
			
			[JsonPropertyName("reason")]
			public string Reason { get; init; }
		}
		
		public sealed class Handler : IRequestHandler<Request, ApiModel>
		{
			private readonly ApiContext _db;
			public Handler(ApiContext db) => _db = db;

			public async Task<ApiModel> Handle(Request request, CancellationToken cancellationToken)
			{
				var entity = new InfractionEntity()
				{
					Key = Guid.NewGuid(),
					Type = request.Type,
					TargetUserId = request.TargetUserId,
					EnforcerUserId = request.EnforcerUserId,
					GuildCreationId = request.GuildCreationId,
					Created = request.Created,
					Expires = request.Expires,
					Reason = request.Reason,
					AddedByFK = request.AddedBy
				};

				_db.Add(entity);
				await _db.SaveChangesAsync(cancellationToken);

				return ToModel(entity);
			}

			private static ApiModel ToModel(InfractionEntity entity)
			{
				return new()
				{
					Key = entity.Key,
					Type = entity.Type,
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
					.WithMessage("guildCreationId: must not be empty.");

				RuleFor(r => r.Created.ToUniversalTime())
					.LessThan(DateTime.UtcNow)
					.WithMessage("created: appears to be in the future.");

				RuleFor(r => r.Expires)
					.LessThan(DateTime.UtcNow)
					.Unless(r => r.Expires is null)
					.WithMessage("expires: appears to be in the past.");
				
				RuleFor(r => r.Reason)
					.MaximumLength(4000)
					.WithMessage("reason: must not exceed 4000 characters.");

				RuleFor(r => r.Type)
					.NotEqual(InfractionType.Ignore);

				RuleFor(r => r.Expires)
					.NotEmpty()
					.Unless(r => r.Type is not InfractionType.SoftBan)
					.WithMessage("expires: temporary bans require an expiration.");
			}
		}
	}
}