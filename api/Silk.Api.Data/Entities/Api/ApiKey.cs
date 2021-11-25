using System;
using Microsoft.EntityFrameworkCore;

namespace Silk.Api.Data.Entities
{
	/// <summary>
	/// An API key used for accessing Silk's API
	/// </summary>
	[Index(nameof(ApiUserId), IsUnique = true)]
	public sealed class ApiKey
	{
		public int Id { get; set; }
		public int ApiUserId { get; set; }
		public DateTime GeneratedAt { get; set; }
		public string DiscordUserId { get; set; }
	}
}