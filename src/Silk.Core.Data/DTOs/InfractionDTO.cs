using System;
using System.ComponentModel.DataAnnotations.Schema;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.DTOs
{
	[NotMapped]
	public sealed record InfractionDTO
	{
		public InfractionDTO(Infraction infraction) 
		{
			Id = infraction.Id;
			UserId = infraction.UserId;
			GuildId = infraction.GuildId;
			EnforcerId = infraction.Enforcer;
			CaseNumber = infraction.CaseNumber;
			HeldAgainstUser = infraction.HeldAgainstUser;
			CreatedAt = infraction.InfractionTime;
			LastUpdated = infraction.LastUpdated;
			Duration = infraction.Expiration - DateTime.UtcNow;
			Expiration = infraction.Expiration;
			Type = infraction.InfractionType;
			EscalatedFromStrike = infraction.EscalatedFromStrike;
			Reason = infraction.Reason;
		}
		
		public int Id { get; init; }
		public ulong UserId { get; init; }
		public ulong GuildId { get; init; }
		public ulong EnforcerId { get; init; }
		public int CaseNumber { get; init; }
		public bool HeldAgainstUser { get; init; }
		public DateTime CreatedAt { get; init; }
		public DateTime? LastUpdated { get; init; }
		public TimeSpan? Duration { get; init; }
		public DateTime? Expiration { get; init; }
		public InfractionType Type { get; init; }
		public bool EscalatedFromStrike { get; init; }
		public string Reason { get; init; }
	}
}