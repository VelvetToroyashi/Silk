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
	public static class GetInfractionByUser
	{
		public sealed record Request(ulong UserId, ulong GuildId) : IRequest<IEnumerable<ApiModel>>;

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
		
		public sealed class Handler : IRequestHandler<Request, IEnumerable<ApiModel>>
		{
			private readonly ApiContext _db;
			public Handler(ApiContext db) => _db = db;
			
			public async Task<IEnumerable<ApiModel>> Handle(Request request, CancellationToken cancellationToken)
			{
				var infractions = await _db.Infractions
					.Where(inf => inf.TargetUserId == request.UserId && inf.GuildCreationId == request.GuildId)
					.ToArrayAsync(cancellationToken);
				
				return infractions?.Select(ToModel) ?? Array.Empty<ApiModel>();
			}

			public static ApiModel ToModel(InfractionEntity entity)
			{
				if (entity is null) return null;
				
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
				RuleFor(req => req.GuildId)
					.NotEmpty();

				RuleFor(req => req.UserId)
					.NotEmpty();
			}
		}
	}
}