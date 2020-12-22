using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Silk.Dashboard.Helpers;
using Silk.Dashboard.Models.Discord;
using Silk.Dashboard.Services.Contracts;

namespace Silk.Dashboard.Services.Concretions
{
    public class DiscordUserService : IDiscordUserService, IDisposable
    {
        private readonly IDiscordHttpClient _discordHttpClient;

        public DiscordUserService(IDiscordHttpClient discordHttpClient)
        {
            _discordHttpClient = discordHttpClient;
        }

        /// <summary>
        /// Gets the user's discord oauth2 access token
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public async Task<string> GetTokenAsync(HttpContext httpContext)
        {
            if (!Authenticated(httpContext.User)) return null;

            var discordToken =
                await httpContext.GetTokenAsync(DiscordConfiguration.AuthenticationScheme, "access_token");
            return discordToken;
        }

        /// <summary>
        /// Parses the user's discord claim for their `identify` information
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public async Task<DiscordApiUser> GetUserInfoAsync(HttpContext httpContext)
        {
            if (!Authenticated(httpContext.User)) return null;

            var token = await GetTokenAsync(httpContext);
            var user = await _discordHttpClient.GetUserAsync(token);

            return user;
        }

        /// <summary>
        /// Gets a list of the user's guilds, Requires `Guilds` scope
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public async Task<List<DiscordApiGuild>> GetUserGuildsAsync(HttpContext httpContext,
            Expression<Func<DiscordApiGuild, bool>> filter = null)
        {
            if (!Authenticated(httpContext.User)) return null;

            try
            {
                var token = await GetTokenAsync(httpContext);
                var guilds = await _discordHttpClient.GetUserGuildsAsync(token);
                if (filter != null) guilds = guilds.Where(filter.Compile()).ToList();
                return guilds;
            }
            catch
            {
                return null;
            }
        }

        public async Task<DiscordApiGuild> GetGuild(HttpContext httpContext, ulong guildId)
        {
            if (!Authenticated(httpContext.User)) return null;

            try
            {
                var token = await GetTokenAsync(httpContext);
                var guild = await _discordHttpClient.GetGuildAsync(token, guildId);
                return guild;
            }
            catch
            {
                return null;
            }
        }

        private bool Authenticated(ClaimsPrincipal claimsPrincipal) =>
            claimsPrincipal.Identity?.IsAuthenticated ?? false;

        public void Dispose()
        {
            _discordHttpClient?.Dispose();
        }
    }
}