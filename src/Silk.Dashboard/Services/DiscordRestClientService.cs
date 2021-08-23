using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;

namespace Silk.Dashboard.Services
{
    public class DiscordRestClientService : IDisposable
    {
        private bool _disposed;

        public DiscordRestClient RestClient { get; }

        public DiscordRestClientService(DiscordRestClient restClient) => RestClient = restClient;

        /* Todo: Remove named arg when library updates default limit (Discord limit updated to 200 guilds) */
        public async Task<IReadOnlyList<DiscordGuild>> GetAllGuildsAsync()
            => await RestClient.GetCurrentUserGuildsAsync(limit: 200);

        public async Task<IReadOnlyList<DiscordGuild>> GetGuildsByPermissionAsync(Permissions perms)
            => FilterGuildsByPermission(await GetAllGuildsAsync(), perms);

        public IReadOnlyList<DiscordGuild> FilterGuildsByPermission(IReadOnlyList<DiscordGuild> guilds, Permissions perms)
            => guilds.Where(g => (g.Permissions & perms) != 0).ToList();

        public async Task<DiscordGuild> GetGuildByIdAndPermissions(ulong guildId, Permissions permissions)
        {
            return (await GetGuildsByPermissionAsync(permissions))
                .FirstOrDefault(g => g.Id == guildId);
        }

        public void Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                RestClient?.Dispose();
            }

            _disposed = true;
        }
    }
}