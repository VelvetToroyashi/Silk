using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Menus;

namespace Silk.Extensions.DSharpPlus
{
    public static class DiscordClientExtensions
    {
        public static DiscordUser? GetMember(this DiscordClient client, Func<DiscordMember, bool> predicate)
        {
            return client
                .Guilds
                .Values
                .SelectMany(g => g.Members.Values)
                .FirstOrDefault(predicate);
        }

        public static DiscordMember? GetMember(this DiscordShardedClient client, Func<DiscordMember, bool> predicate)
        {
            return client
                .ShardClients
                .Values
                .SelectMany(c => c.Guilds.Values)
                .SelectMany(g => g.Members.Values)
                .FirstOrDefault(predicate);
        }

        public static Task<IReadOnlyDictionary<int, MenusExtension>> UseMenusAsync(this DiscordShardedClient client)
        {
            var dict = new Dictionary<int, MenusExtension>();

            foreach (var shard in client.ShardClients)
                dict[shard.Key] = shard.Value.UseMenus();

            return Task.FromResult((IReadOnlyDictionary<int, MenusExtension>)dict);
        }
        
    }
}