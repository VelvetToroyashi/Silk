using System.Text.Json.Serialization;

namespace Silk_Dashboard.Models.Discord
{
    public class DiscordApiRole
    {
        [JsonPropertyName("id")] public string Id { get; set; }

        [JsonPropertyName("name")] public string Name { get; set; }

        [JsonPropertyName("permissions")] public string Permissions { get; set; }

        [JsonPropertyName("position")] public int Position { get; set; }

        [JsonPropertyName("color")] public int Color { get; set; }

        [JsonPropertyName("hoist")] public bool Hoist { get; set; }

        [JsonPropertyName("managed")] public bool Managed { get; set; }

        [JsonPropertyName("mentionable")] public bool Mentionable { get; set; }
    }
}