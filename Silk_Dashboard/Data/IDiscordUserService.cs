using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Silk_Dashboard.Models;

namespace Silk_Dashboard.Data
{
    public interface IDiscordUserService
    {
        /// <summary>
        /// Parses the user's discord claim for their `identify` information
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        DiscordUserClaim GetUserInfo(HttpContext httpContext);

        /// <summary>
        /// Gets the user's discord oauth2 access token
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        Task<string> GetTokenAsync(HttpContext httpContext);

        // TODO: Add Paging for Guilds (don't return ALL - could be massive amount)
        /// <summary>
        /// Gets a list of the user's guilds, Requires `Guilds` scope
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        Task<List<Guild>> GetUserGuildsAsync(HttpContext httpContext, Expression<Func<Guild, bool>> filter = null);
    }
}