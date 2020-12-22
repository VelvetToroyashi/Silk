#region

using DSharpPlus.Entities;

#endregion

namespace SilkBot.Extensions.DSharpPlus
{
    public static class DiscordUserExtensions
    {
        public static string GetUrl(this DiscordUser user)
        {
            return $"https://discord.com/users/{user.Id}";
        }
    }
}