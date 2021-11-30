using System;
using System.Collections.Generic;

namespace Silk.Core.Data.Entities
{
	public class UserEntity
	{
		public ulong Id { get; set; }

		public long DatabaseId { get; set; }

		public ulong GuildId { get; set; }

		public GuildEntity Guild { get; set; } = null!;

		public UserFlag Flags { get; set; }

		public DateTime InitialJoinDate { get; set; }

		public UserHistoryEntity History { get; set; }

		public List<InfractionEntity> Infractions { get; set; }
		//public List<Reminder> Reminders { get; set; } = new();
	}
}