using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Silk_Dashboard.Models.Discord
{
    public class DiscordApiGuild
    {
        [JsonPropertyName("id")] public string Id { get; set; }

        [JsonPropertyName("name")] public string Name { get; set; }

        [JsonPropertyName("icon")] public string Icon { get; set; }

        [JsonPropertyName("description")] public string Description { get; set; }

        [JsonPropertyName("splash")] public object Splash { get; set; }

        [JsonPropertyName("discovery_splash")] public object DiscoverySplash { get; set; }

        [JsonPropertyName("permissions")] public ulong Permissions { get; set; }

        [JsonPropertyName("features")] public List<string> Features { get; set; }

        [JsonPropertyName("emojis")] public List<DiscordApiEmoji> Emojis { get; set; }

        [JsonPropertyName("banner")] public string Banner { get; set; }

        [JsonPropertyName("owner_id")] public string OwnerId { get; set; }

        [JsonPropertyName("application_id")] public object ApplicationId { get; set; }

        [JsonPropertyName("region")] public string Region { get; set; }

        [JsonPropertyName("afk_channel_id")] public object AfkChannelId { get; set; }

        [JsonPropertyName("afk_timeout")] public int AfkTimeout { get; set; }

        [JsonPropertyName("system_channel_id")]
        public object SystemChannelId { get; set; }

        [JsonPropertyName("widget_enabled")] public bool WidgetEnabled { get; set; }

        [JsonPropertyName("widget_channel_id")]
        public object WidgetChannelId { get; set; }

        [JsonPropertyName("verification_level")]
        public int VerificationLevel { get; set; }

        [JsonPropertyName("roles")] public List<DiscordApiRole> Roles { get; set; }

        [JsonPropertyName("default_message_notifications")]
        public int DefaultMessageNotifications { get; set; }

        [JsonPropertyName("mfa_level")] public int MfaLevel { get; set; }

        [JsonPropertyName("explicit_content_filter")]
        public int ExplicitContentFilter { get; set; }

        [JsonPropertyName("max_presences")] public int MaxPresences { get; set; }

        [JsonPropertyName("max_members")] public int MaxMembers { get; set; }

        [JsonPropertyName("vanity_url_code")] public string VanityUrlCode { get; set; }

        [JsonPropertyName("premium_tier")] public int PremiumTier { get; set; }

        [JsonPropertyName("premium_subscription_count")]
        public int PremiumSubscriptionCount { get; set; }

        [JsonPropertyName("system_channel_flags")]
        public int SystemChannelFlags { get; set; }

        [JsonPropertyName("preferred_locale")] public string PreferredLocale { get; set; }

        [JsonPropertyName("rules_channel_id")] public string RulesChannelId { get; set; }

        [JsonPropertyName("public_updates_channel_id")]
        public string PublicUpdatesChannelId { get; set; }
    }
}