using Silk.Core.Data.Models;

namespace Silk.Core.Data.DTOs
{
	public sealed record InfractionDTO
	{
		public InfractionDTO(Infraction infraction)
			: this(infraction.Id, infraction.UserId, infraction.GuildId, infraction.Enforcer, infraction.InfractionType, infraction.Reason, !infraction.HeldAgainstUser) { }
		public InfractionDTO(int id, ulong userId, ulong guildId, ulong enforcerId, InfractionType type, string reason, bool rescinded)
		{
			Id = id;
			UserId = userId;
			GuildId = guildId;
			Type = type;
			Reason = reason;
			Rescinded = rescinded;
			EnforcerId = enforcerId;
		}
		public int Id { get; init; }
		public ulong UserId { get; init; }
		public ulong GuildId { get; init; }
		public ulong EnforcerId { get; init; }
		public bool Rescinded { get; init; }
		public InfractionType Type { get; init; }
		public string Reason { get; init; }
	}
}