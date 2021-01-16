using System.Net.Http;

namespace Silk.Core.Utilities
{
    public static class HttpClientExtensions
    {
        public static HttpClient CreateSilkClient(this IHttpClientFactory httpClientFactory)
        {
            return httpClientFactory.CreateClient(Program.HttpClientName);
        }
    }
}