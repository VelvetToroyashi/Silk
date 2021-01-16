using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Silk.Dashboard.Models.Discord;
using Silk.Dashboard.Services.Contracts;

namespace Silk.Dashboard.Services.Concretions
{
    /*
     * Adapted from: https://github.com/GedasFX/Alderto/blob/master/Alderto.Web/Services/DiscordHttpClient.cs
     */
    public class DiscordHttpClient : IDiscordHttpClient
    {
        private static readonly string UserUrl = "https://discord.com/api/v6/users/@me";
        private static readonly string UserGuildsUrl = "https://discord.com/api/v6/users/@me/guilds";

        private static JsonSerializerOptions DefaultOptions { get; } = new() {PropertyNameCaseInsensitive = true};

        private readonly HttpClient _httpClient;

        public DiscordHttpClient(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        public Task<DiscordApiUser> GetUserAsync(string accessToken)
            => GetResourceAsync<DiscordApiUser>(accessToken, UserUrl, DefaultOptions);

        public Task<List<DiscordApiGuild>> GetUserGuildsAsync(string accessToken)
            => GetResourceAsync<List<DiscordApiGuild>>(accessToken, UserGuildsUrl, DefaultOptions);

        public Task<DiscordApiGuild> GetGuildAsync(string accessToken, ulong guildId)
        {
            throw new System.NotImplementedException();
        }

        private async Task<T> GetResourceAsync<T>(string discordAccessToken, string url, JsonSerializerOptions serializerOptions = null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", discordAccessToken);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return default(T);

            var responseContent = await response.Content.ReadAsStringAsync();
            
            var resource = JsonSerializer.Deserialize<T>(responseContent, serializerOptions);
            return resource;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
