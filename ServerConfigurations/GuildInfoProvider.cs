using DSharpPlus.Entities;
using System.Collections.Generic;

namespace SilkBot.ServerConfigurations
{
    public class GuildInfoProvider : IGuildInfoProvider
    {
        public GuildInfo this[DiscordGuild guild] { get => guildInfos[guild.Id]; }
        private readonly Dictionary<ulong, GuildInfo> guildInfos = new Dictionary<ulong, GuildInfo>();
    }
}
