using System.Net.Http;
using Silk.Shared.Constants;

namespace Silk.Core.Utilities.HttpClient;

public static class HttpClientExtensions
{
    public static System.Net.Http.HttpClient CreateSilkClient(this IHttpClientFactory httpClientFactory) => httpClientFactory.CreateClient(StringConstants.HttpClientName);
}