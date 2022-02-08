using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Rest.Core;

namespace Silk.Dashboard.Services;

public class DiscordRestClientService
{
    public IDiscordRestUserAPI RestClient { get; }

    public DiscordRestClientService(IDiscordRestUserAPI restClient) 
        => RestClient = restClient;

    public async Task<IReadOnlyList<IPartialGuild>> GetAllGuildsAsync()
        => (await RestClient.GetCurrentUserGuildsAsync(limit: 100)).Entity;

    public async Task<IReadOnlyList<IPartialGuild>> GetGuildsByPermissionAsync(
        DiscordPermission permissions)
        => FilterGuildsByPermission(await GetAllGuildsAsync(), permissions);

    public IReadOnlyList<IPartialGuild> FilterGuildsByPermission(
        IReadOnlyList<IPartialGuild> guilds, 
        DiscordPermission            permission)
        => guilds.Where(g => g.Permissions.IsDefined(out var permissionSet) && 
                             permissionSet.HasPermission(permission)).ToList();
        
    public async Task<IPartialGuild> GetGuildByIdAndPermissionAsync(
        Snowflake         guildId, 
        DiscordPermission permission)
    {
        return (await GetGuildsByPermissionAsync(permission))
           .FirstOrDefault(g => g.ID == guildId)!;
    }
}