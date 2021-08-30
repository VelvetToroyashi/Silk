using System;
using System.Text.Json.Serialization;

namespace Silk.Api.Data.Entities
{
	public sealed class ApiUser
	{
		public int Id { get; set; }

		public DateTime ApiKeyGenerationTimestamp { get; set; }
		public string Username { get; set; }
		
		public string PasswordHash { get; set; }
		
		[JsonIgnore]
		public string PasswordSalt { get; set; }
	}
}