namespace Silk.Api.Helpers
{
	/// <summary>
	/// API related settings.
	/// </summary>
	public sealed record ApiSettings 
	{
		/// <summary>
		/// Cryptographic secret for signing JWT tokens.
		/// </summary>
		public string JwtSecret { get; init; }
	
		/// <summary>
		/// Cryptographic salt for hashing passwords.
		/// </summary>
		public string HashSalt { get; init; }
	}
}