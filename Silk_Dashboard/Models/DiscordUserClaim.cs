namespace Silk_Dashboard.Models
{
    public class DiscordUserClaim
    {
        public ulong UserId { get; set; }
        public string Name { get; set; }
        public string Discriminator { get; set; }
        public string Avatar { get; set; }

        /// <summary>
        /// Will be null if the email scope is not provided
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Whether the email on this account has been verified, can be null
        /// </summary>
        public bool? Verified { get; set; }
    }
}