using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Silk_Dashboard.Models
{
    public class Guild
    {
        [JsonProperty("id")] public string Id { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("icon")] public string Icon { get; set; }

        [JsonProperty("owner")] public bool Owner { get; set; }

        [JsonProperty("permissions")] public ulong Permissions { get; set; }

        [JsonProperty("features")] public List<string> Features { get; set; }

        public static List<Guild> ListFromJson(string json) =>
            JsonConvert.DeserializeObject<List<Guild>>(json, Settings);

        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter {DateTimeStyles = DateTimeStyles.AssumeUniversal}
            },
        };
    }
}