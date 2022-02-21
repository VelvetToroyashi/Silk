namespace Silk.Dashboard.Models;

/* Todo: Move this when ready to the Shared project (keeping here for now) */
public record SilkContributor(string Name, string Description, string ImageUrl)
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
}