using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Silk.Dashboard.Models.Discord;

namespace Silk.Dashboard.Services.Contracts
{
    public interface IDiscordUserService
    {
        /// <summary>
        /// Gets the user's discord oauth2 access token
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        Task<string> GetTokenAsync(HttpContext httpContext);
        
        /// <summary>
        /// Parses the user's discord claim for their `identify` information
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        Task<DiscordApiUser> GetUserInfoAsync(HttpContext httpContext);

        // TODO: Add Paging for Guilds (don't return ALL - could be massive amount)
        /// <summary>
        /// Gets a list of the user's guilds, Requires `Guilds` scope
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<List<DiscordApiGuild>> GetUserGuildsAsync(HttpContext httpContext, Expression<Func<DiscordApiGuild, bool>> filter = null);

        Task<DiscordApiGuild> GetGuild(HttpContext httpContext, ulong guildId);
    }
}