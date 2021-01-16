using AspNet.Security.OAuth.Discord;

namespace Silk.Dashboard.Helpers
{
    public static class DiscordConfiguration
    {
        public static string AuthenticationScheme { get; } = DiscordAuthenticationDefaults.AuthenticationScheme;
        public static string OAuth2CallbackPath { get; } = "/signin-discord";
    }
}