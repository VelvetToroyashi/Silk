using System.Text.Json.Serialization;

namespace Silk.Dashboard.Models.Discord
{
    public class DiscordApiUser
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } 

        [JsonPropertyName("username")]
        public string Username { get; set; } 

        [JsonPropertyName("discriminator")]
        public string Discriminator { get; set; } 

        [JsonPropertyName("avatar")]
        public string Avatar { get; set; }

        [JsonPropertyName("bot")]
        public bool? Bot { get; set; }
            
        [JsonPropertyName("mfa_enabled")]
        public bool? MfaEnabled { get; set; }
            
        [JsonPropertyName("locale")]
        public string? Locale { get; set; }

        [JsonPropertyName("verified")]
        public bool? Verified { get; set; } 

        [JsonPropertyName("email")]
        public string? Email { get; set; } 

        [JsonPropertyName("flags")]
        public int? Flags { get; set; } 

        [JsonPropertyName("premium_type")]
        public int? PremiumType { get; set; } 

        [JsonPropertyName("public_flags")]
        public int? PublicFlags { get; set; } 
    }
}