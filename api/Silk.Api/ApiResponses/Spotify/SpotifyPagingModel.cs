using System.Collections.Generic;
using Newtonsoft.Json;

namespace Silk.Api.ApiResponses.Spotify
{
    public class SpotifyPagingModel<TItemModel>
    {
        [JsonProperty("href")]
        public string SearchUrl { get; set; }
        
        [JsonProperty("items")]
        public List<TItemModel> Items { get; set; }
        
        [JsonProperty("limit")]
        public int Limit { get; set; }
        
        [JsonProperty("next")]
        public string NextPageUrl { get; set; }
        
        [JsonProperty("offset")]
        public int Offset { get; set; }
        
        [JsonProperty("previous")]
        public string PreviousPageUrl { get; set; }
        
        [JsonProperty("total")]
        public int Total { get; set; }
    }
}