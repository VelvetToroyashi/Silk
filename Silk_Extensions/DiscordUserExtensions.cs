using DSharpPlus.Entities;

namespace SilkBot.Extensions
{
    public static class DiscordUserExtensions
    {
        public static string GetUrl(this DiscordUser user) => $"https://discord.com/users/{user.Id}";
    }
}
