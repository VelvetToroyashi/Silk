using System.Text.Json.Serialization;

namespace Silk.Dashboard.Models;

/* Todo: Move this when ready to the Shared project (keeping here for now) */
public record SilkContributor
{
    [JsonPropertyName("login")]
    public string Name { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("avatar_url")]
    public string AvatarUrl { get; set; }

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; }

    [JsonPropertyName("contributions")]
    public int Contributions { get; set; }

    [JsonIgnore]
    public string Description { get; set; } = "";
}