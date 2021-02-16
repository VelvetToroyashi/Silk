using System;
using System.Linq;
using DSharpPlus;
using DSharpPlus.Entities;

namespace Silk.Extensions.DSharpPlus
{
    public static class DiscordClientExtensions
    {
        public static DiscordUser GetUser(this DiscordClient client, Func<DiscordMember, bool> predicate) => 
            client
            .Guilds
            .Values
            .SelectMany(g => g.Members.Values)
            .FirstOrDefault(predicate);
        
        public static DiscordMember GetUser(this DiscordShardedClient client, Func<DiscordMember, bool> predicate) =>
            client
            .ShardClients
            .Values
            .SelectMany(c => c.Guilds.Values)
            .SelectMany(g => g.Members.Values)
            .FirstOrDefault(predicate);
    }
}