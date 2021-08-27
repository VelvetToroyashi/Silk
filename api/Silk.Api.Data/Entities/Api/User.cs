using System;
using System.Text.Json.Serialization;

namespace Silk.Api.Data.Entities
{
	public sealed class User
	{
		public int Id { get; set; }
		public Guid Key { get; set; }
		public string Username { get; set; }
		
		[JsonIgnore]
		public string Password { get; set; }
	}
}