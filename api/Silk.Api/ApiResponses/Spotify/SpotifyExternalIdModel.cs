using Newtonsoft.Json;

namespace Silk.Api.ApiResponses.Spotify
{
    public class SpotifyExternalIdModel
    {
        [JsonProperty("ean")]
        public string InternationalArticleNumber { get; set; }
        
        [JsonProperty("isrc")]
        public string InternationalStandardRecordingCode { get; set; }
        
        [JsonProperty("upc")]
        public string UniversalProductCode { get; set; }
    }
}