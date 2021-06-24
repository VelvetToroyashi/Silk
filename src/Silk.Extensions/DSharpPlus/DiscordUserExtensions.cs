using DSharpPlus.Entities;

namespace Silk.Extensions.DSharpPlus
{
    public static class DiscordUserExtensions
    {
        public static string GetUrl(this DiscordUser user) => $"https://discord.com/users/{user.Id}";
        public static string ToDiscordName(this DiscordUser user) => $"{user.Username}#{user.Discriminator}";
    }
}