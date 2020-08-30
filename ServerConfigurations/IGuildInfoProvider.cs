using DSharpPlus.Entities;

namespace SilkBot.ServerConfigurations
{
    public interface IGuildInfoProvider
    {
        GuildInfo this[DiscordGuild guild] { get; }
    }
}