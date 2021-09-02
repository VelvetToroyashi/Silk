using Newtonsoft.Json;

namespace Silk.Api.ApiResponses.Spotify
{
    public class SpotifyTrackModel
    {
        [JsonProperty("duration_ms")]
        public int DurationMilliseconds { get; set; }
        
        [JsonProperty("href")]
        public string TrackUrl { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}