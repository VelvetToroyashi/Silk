using System.Collections.Generic;
using Newtonsoft.Json;

namespace Silk.Api.ApiResponses.Spotify
{
    public class SpotifyTrackModel
    {
        [JsonProperty("album")]
        public SpotifySimplifiedAlbumModel Album { get; set; }
        
        [JsonProperty("artists")]
        public List<SpotifyArtistModel> Artists { get; set; }
        
        [JsonProperty("available_markets")]
        public List<string> AvailableMarkets { get; set; }
        
        [JsonProperty("disc_number")]
        public int DiscNumber { get; set; }
        
        [JsonProperty("duration_ms")]
        public int DurationMilliseconds { get; set; }
        
        [JsonProperty("explicit")]
        public bool Explicit { get; set; }
        
        [JsonProperty("external_ids")]
        public SpotifyExternalIdModel ExternalIds { get; set; }
        
        [JsonProperty("external_urls")]
        public SpotifyExternalUrlObject ExternalUrls { get; set; }
        
        [JsonProperty("href")]
        public string TrackUrl { get; set; }
        
        [JsonProperty("id")]
        public string SpotifyId { get; set; }
        
        [JsonProperty("is_local")]
        public bool IsLocal { get; set; }
        
        [JsonProperty("is_playable")]
        public bool IsPlayable { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("popularity")]
        public int Popularity { get; set; }
        
        [JsonProperty("preview_url")]
        public string PreviewUrl { get; set; }
        
        [JsonProperty("restrictions")]
        public object Restrictions { get; set; }
        
        [JsonProperty("track_number")]
        public int TrackNumber { get; set; }
        
        [JsonProperty("type")]
        public string Type { get; set; }
        
        [JsonProperty("uri")]
        public string LaunchUri { get; set; }
    }
}