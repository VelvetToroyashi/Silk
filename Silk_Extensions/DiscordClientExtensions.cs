using System.Linq;
using DSharpPlus;
using DSharpPlus.Entities;

namespace Silk__Extensions
{
    public static class DiscordClientExtensions
    {
        public static DiscordUser GetUser(this DiscordClient c, string u)
        {
            foreach (DiscordGuild g in c.Guilds.Values)
            {
                foreach (DiscordMember m in g.Members.Values)
                {
                    if (m.Username.ToLower().Contains(u.ToLower())) return m;
                }
            }
            return null;
        }

        public static DiscordUser GetUser(this DiscordShardedClient c, string u)
        {
            foreach (DiscordGuild g in c.ShardClients.Values.SelectMany(c => c.Guilds.Values))
            {
                foreach (DiscordMember m in g.Members.Values)
                {
                    if (m.Username.ToLower().Contains(u.ToLower())) return m;
                }
            }
            return null;
        }
        public static DiscordUser GetUser(this DiscordShardedClient c, ulong u)
        {
            foreach (DiscordGuild g in c.ShardClients.Values.SelectMany(c => c.Guilds.Values))
            {
                foreach (DiscordMember m in g.Members.Values)
                {
                    if (m.Id == u) return m;
                }
            }
            return null;
        }
    }
}
