using Microsoft.EntityFrameworkCore;

namespace Silk.Api.Data.Entities
{
	/// <summary>
	/// An API key used for accessing Silk's API
	/// </summary>
	[Index(nameof(KeyHash), IsUnique = true)]
	public sealed class ApiKey
	{
		public int Id { get; set; }
		public int UserId { get; set; }
		public bool Revoked { get; set; }
		public string KeyHash { get; set; }
	}
}