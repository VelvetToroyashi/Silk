using System;
using System.Linq;
using DSharpPlus;
using DSharpPlus.Entities;

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
    }
}