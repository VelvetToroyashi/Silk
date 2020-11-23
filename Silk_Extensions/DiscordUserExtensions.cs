using DSharpPlus.Entities;

namespace SilkBot.Extensions
{
    public static class DiscordUserExtensions
    {
        public static string GetUrl(this DiscordUser user)
        {
            return $"https://discord.com/users/{user.Id}";
        }
    }
}