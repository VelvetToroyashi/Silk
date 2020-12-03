namespace Silk_Dashboard.Discord.OAuth2
{
    public static class DiscordDefaults
    {
        public const string AuthenticationScheme = "Discord";
        public const string DisplayName = "Discord";

        public static readonly string AuthorizationEndpoint = "https://discord.com/api/oauth2/authorize";
        public static readonly string TokenEndpoint = "https://discord.com/api/oauth2/token";
        public static readonly string UserInformationEndpoint = "https://discord.com/api/users/@me";
    }
}