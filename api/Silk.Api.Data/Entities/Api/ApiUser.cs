using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Silk.Api.Data.Models;

namespace Silk.Api.Data.Entities
{
	public sealed class ApiUser
	{
		public ApiKey ApiKey { get; set; }

		[Key]
		public string DiscordId { get; set; }

		public DateTime ApiKeyGenerationTimestamp { get; set; }

		public ICollection<InfractionEntity> Infractions { get; set; }
	}
}