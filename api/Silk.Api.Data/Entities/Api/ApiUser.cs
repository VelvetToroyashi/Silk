using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;

namespace Silk.Api.Data.Entities
{
	public sealed class ApiUser : IdentityUser
	{
		public int Id { get; set; }

		public ApiKey Key { get; set; }
		public string Username { get; set; }
		
		[JsonIgnore]
		public string PasswordSalt { get; set; }
	}
}