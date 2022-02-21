#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;
using Silk.Dashboard.Models;
using Silk.Shared.Constants;

namespace Silk.Dashboard.Pages;

public partial class About
{
    [Inject]
    private IHttpClientFactory HttpClientFactory { get; set; }

    private const string ContributorsUrl         = "https://api.github.com/repos/VTPDevelopment/Silk/contributors";
    private const string ExcludedContributorName = "[bot]";

    private static readonly TimeSpan       _fetchPeriod = TimeSpan.FromHours(1);
    private static          DateTimeOffset _nextFetchTime;
    
    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        AllowTrailingCommas         = true,
        DefaultIgnoreCondition      = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
    };

    // Todo: Paginate GET contributors (defaults to ~30)
    private static List<SilkContributor>? _contributors;

    protected override async Task OnInitializedAsync()
    {
        await GetContributorsAsync();
    }

    private async Task GetContributorsAsync()
    {
        if (_contributors is null)
        {
            await FetchContributorsAsync();
        }
        else
        {
            if (_nextFetchTime < DateTimeOffset.UtcNow)
                await FetchContributorsAsync();
        }

        _contributors = _contributors?.Where(contributor => !contributor.Name.Contains(ExcludedContributorName))
                                      .OrderByDescending(contributor => contributor.Contributions)
                                      .ToList();
    }

    private async Task FetchContributorsAsync()
    {
        using var client  = HttpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, ContributorsUrl);

        request.Headers.TryAddWithoutValidation("Accept", "application/vnd.github.v3+json");
        request.Headers.TryAddWithoutValidation("User-Agent", StringConstants.ProjectIdentifier);

        using var result = await client.SendAsync(request);

        if (!result.IsSuccessStatusCode) return;

        _contributors  = JsonSerializer.Deserialize<List<SilkContributor>>(await result.Content.ReadAsStringAsync(), 
                                                                           _serializerOptions);
        _nextFetchTime = DateTimeOffset.UtcNow + _fetchPeriod;
    }
}