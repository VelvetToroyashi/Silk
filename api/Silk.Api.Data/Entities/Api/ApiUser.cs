using System;
using System.ComponentModel.DataAnnotations;

namespace Silk.Api.Data.Entities
{
	public sealed class ApiUser
	{
		public ApiKey ApiKey { get; set; }
		
		[Key]
		public string DiscordId { get; set; }

		public DateTime ApiKeyGenerationTimestamp { get; set; }
	}
}