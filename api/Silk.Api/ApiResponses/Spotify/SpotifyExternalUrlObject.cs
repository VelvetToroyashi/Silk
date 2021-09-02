using Newtonsoft.Json;

namespace Silk.Api.ApiResponses.Spotify
{
    public class SpotifyExternalUrlObject
    {
        [JsonProperty("spotify")]
        public string Spotify { get; set; }
    }
}