using System;
using System.ComponentModel.DataAnnotations.Schema;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.DTOs
{
	[NotMapped]
	public sealed record InfractionDTO
	{
		public InfractionDTO(Infraction infraction)
			: this(infraction.Id, infraction.UserId, 
				infraction.GuildId, infraction.Enforcer, 
				infraction.InfractionType, infraction.Reason, 
				!infraction.HeldAgainstUser, infraction.InfractionTime,
				infraction.CaseNumber, infraction.Expiration) { }
		public InfractionDTO(
			int id, ulong userId,
			ulong guildId, ulong enforcerId, 
			InfractionType type, string reason, 
			bool rescinded, DateTime createdAt, 
			int caseNumber, DateTime? expiration = null, 
			DateTime? lastUpdated = null)
		{
			Id = id;
			UserId = userId;
			GuildId = guildId;
			CaseNumber = caseNumber;
			Type = type;
			Reason = reason;
			Rescinded = rescinded;
			EnforcerId = enforcerId;
			CreatedAt = createdAt;
			Duration = expiration is null ? null : expiration - (lastUpdated ?? createdAt);
			LastUpdated = lastUpdated;
			Expiration = expiration;
		}
		

		public int Id { get; init; }
		public ulong UserId { get; init; }
		public ulong GuildId { get; init; }
		public ulong EnforcerId { get; init; }
		public int CaseNumber { get; init; }
		public bool Rescinded { get; init; }
		public DateTime CreatedAt { get; init; }
		public DateTime? LastUpdated { get; init; }
		public TimeSpan? Duration { get; init; }
		public DateTime? Expiration { get; init; }
		public InfractionType Type { get; init; }
		public string Reason { get; init; }
		
	}
}