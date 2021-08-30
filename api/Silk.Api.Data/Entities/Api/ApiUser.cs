using System;

namespace Silk.Api.Data.Entities
{
	public sealed class ApiUser
	{
		public int Id { get; set; }
		
		public ApiKey ApiKey { get; set; }
		
		public string DiscordId { get; set; }

		public DateTime ApiKeyGenerationTimestamp { get; set; }
		
		
	}
}