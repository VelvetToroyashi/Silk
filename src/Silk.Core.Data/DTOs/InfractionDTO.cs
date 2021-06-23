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
				infraction.Expiration - DateTime.UtcNow) { }
		public InfractionDTO(int id, ulong userId, ulong guildId, ulong enforcerId, InfractionType type, string reason, bool rescinded, DateTime createdAt, TimeSpan? duration = null)
		{
			Id = id;
			UserId = userId;
			GuildId = guildId;
			Type = type;
			Reason = reason;
			Rescinded = rescinded;
			EnforcerId = enforcerId;
			CreatedAt = createdAt;
			Duration = duration;
		}
		
		public int Id { get; init; }
		public ulong UserId { get; init; }
		public ulong GuildId { get; init; }
		public ulong EnforcerId { get; init; }
		public bool Rescinded { get; init; }
		public DateTime CreatedAt { get; init; }
		public TimeSpan? Duration { get; init; }
		public InfractionType Type { get; init; }
		public string Reason { get; init; }
	}
}