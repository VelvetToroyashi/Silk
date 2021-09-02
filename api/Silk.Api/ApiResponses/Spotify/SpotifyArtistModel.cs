using System.Collections.Generic;
using Newtonsoft.Json;

namespace Silk.Api.ApiResponses.Spotify
{
    public class SpotifyArtistModel
    {
        [JsonProperty("external_urls")]
        public SpotifyExternalUrlObject ExternalUrls { get; set; }
        
        [JsonProperty("followers")]
        public object Followers { get; set; }
        
        [JsonProperty("genres")]
        public List<string> Genres { get; set; }
        
        [JsonProperty("href")]
        public string ArtistUrl { get; set; }
        
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("images")]
        public List<SpotifyImageObject> Images { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("popularity")]
        public int Popularity { get; set; }
        
        [JsonProperty("type")]
        public string Type { get; set; }
        
        [JsonProperty("uri")]
        public string LaunchUri { get; set; }
    }
}