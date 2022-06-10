#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;
using Silk.Dashboard.Models;

namespace Silk.Dashboard.Pages;

public partial class About
{
    private const string ContributorsUrl         = "https://api.github.com/repos/VTPDevelopment/Silk/contributors";
    private const string ExcludedContributorName = "[bot]";

    private static readonly TimeSpan FetchPeriod = TimeSpan.FromHours(1);

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        AllowTrailingCommas         = true,
        DefaultIgnoreCondition      = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
    };

    private static DateTimeOffset         _nextFetchTime;
    private static List<SilkContributor>? _contributors;

    [Inject]
    private IHttpClientFactory HttpClientFactory { get; set; }

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
        using var client = HttpClientFactory.CreateClient("Github");

        _contributors = await client.GetFromJsonAsync<List<SilkContributor>>(ContributorsUrl, SerializerOptions);

        _nextFetchTime = DateTimeOffset.UtcNow + FetchPeriod;
    }
}