using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Silk_Dashboard.Discord.OAuth2;
using Silk_Dashboard.Models;

namespace Silk_Dashboard.Data
{
    public class DiscordUserService : IDiscordUserService, IDisposable
    {
        private HttpClient _httpClient;
        private IHttpClientFactory _httpClientFactory;

        public DiscordUserService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _httpClient = httpClientFactory.CreateClient();
        }

        /// <summary>
        /// Parses the user's discord claim for their `identify` information
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public DiscordUserClaim GetUserInfo(HttpContext httpContext)
        {
            if (!httpContext.User.Identity.IsAuthenticated)
            {
                return null;
            }

            var claims = httpContext.User.Claims.ToList();
            bool? verified;
            if (bool.TryParse(claims.FirstOrDefault(x => x.Type == "urn:discord:verified")?.Value, out var _verified))
            {
                verified = _verified;
            }
            else
            {
                verified = null;
            }

            var userClaim = new DiscordUserClaim
            {
                UserId = ulong.Parse(claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value),
                Name = claims.First(x => x.Type == ClaimTypes.Name).Value,
                Discriminator = claims.First(x => x.Type == "urn:discord:discriminator").Value,
                Avatar = claims.First(x => x.Type == "urn:discord:avatar").Value,
                Email = claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value,
                Verified = verified
            };

            return userClaim;
        }

        /// <summary>
        /// Gets the user's discord oauth2 access token
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public async Task<string> GetTokenAsync(HttpContext httpContext)
        {
            if (!httpContext.User.Identity.IsAuthenticated)
            {
                return null;
            }

            var discordToken = await httpContext.GetTokenAsync("Discord", "access_token");
            return discordToken;
        }

        /// <summary>
        /// Gets a list of the user's guilds, Requires `Guilds` scope
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public async Task<List<Guild>> GetUserGuildsAsync(HttpContext httpContext, Expression<Func<Guild, bool>> filter = null)
        {
            if (!httpContext.User.Identity.IsAuthenticated)
            {
                return null;
            }

            var token = await GetTokenAsync(httpContext);

            var guildEndpoint = DiscordDefaults.UserInformationEndpoint + "/guilds";

            using var request = new HttpRequestMessage(HttpMethod.Get, guildEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();

            try
            {
                var guilds = Guild.ListFromJson(content);
                if (filter != null) guilds = guilds.Where(filter.Compile()).ToList();
                return guilds;
            }
            catch
            {
                return null;
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}