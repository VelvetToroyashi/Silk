#region

using System;
using System.Linq;
using System.Linq.Expressions;
using DSharpPlus;
using DSharpPlus.Entities;

#endregion

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