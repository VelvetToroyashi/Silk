using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Silk.Core.Data.Entities
{
	public class GuildEntity
	{
		public ulong Id { get; set; }

		[Required]
		[StringLength(5)]
		public string Prefix { get; set; } = "";
		public GuildConfigEntity Configuration { get; set; } = new();
		public GuildModConfigEntity ModConfig { get; set; } = new();
		public List<UserEntity> Users { get; set; } = new();
		public List<InfractionEntity> Infractions { get; set; } = new();
		public List<TagEntity> Tags { get; set; } = new();
	}
}