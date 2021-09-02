using System.Collections.Generic;
using Newtonsoft.Json;

namespace Silk.Api.ApiResponses.Spotify
{
    public class SpotifySimplifiedAlbumModel
    {
        [JsonProperty("album_group")]
        public string Group { get; set; }
        
        [JsonProperty("album_type")]
        public string AlbumType { get; set; }
        
        [JsonProperty("artists")]
        public List<object> Artists { get; set; }
        
        [JsonProperty("available_markets")]
        public List<string> AvailableMarkets { get; set; }
        
        [JsonProperty("external_urls")]
        public SpotifyExternalUrlObject ExternalUrls { get; set; }
        
        [JsonProperty("href")]
        public string AlbumUrl { get; set; }
        
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("images")]
        public List<SpotifyImageObject> Images { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("release_date")]
        public string ReleaseDate { get; set; }
        
        [JsonProperty("release_date_precision")]
        public string ReleaseDatePrecisionUnit { get; set; }
        
        [JsonProperty("restrictions")]
        public object Restriction { get; set; }
        
        [JsonProperty("total_tracks")]
        public int TrackCount { get; set; }
        
        [JsonProperty("type")]
        public string Type { get; set; }
        
        [JsonProperty("uri")]
        public string LaunchUri { get; set; }
    }
}