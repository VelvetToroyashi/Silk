using Newtonsoft.Json;

namespace Silk.Api.ApiResponses.Spotify
{
    public class SpotifyTokenModel
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
        
        [JsonProperty("expires_in")]
        public int TokenExpiry { get; set; }
        
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
        
        [JsonProperty("scope")]
        public string TokenScopes { get; set; }
    }
}