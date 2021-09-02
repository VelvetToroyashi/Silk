using Newtonsoft.Json;

namespace Silk.Api.ApiResponses.Spotify
{
    public class SpotifyImageObject
    {
        [JsonProperty("width")]
        public int Width { get; set; }
        
        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}