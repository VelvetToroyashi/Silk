using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Silk_Dashboard.Models.Discord;

namespace Silk_Dashboard.Services.Contracts
{
    public interface IDiscordHttpClient : IDisposable
    {
        Task<DiscordApiUser> GetUserAsync(string accessToken);
        Task<List<DiscordApiGuild>> GetUserGuildsAsync(string accessToken);
        Task<DiscordApiGuild> GetGuildAsync(string accessToken, ulong guildId);
    }
}