using Remora.Discord.API.Objects;
using Remora.Rest.Core;

namespace Silk.Remora.RedisCache;

public static class RedisKeyHelper
{
    public static string CreateChannelCacheKey(in Snowflake channelID) => $"Channel:{channelID}";
    
    public static string CreatePinnedMessagesCacheKey(in Snowflake channelID) => $"Channel:{channelID}:Messages:Pinned";
    
    public static string CreateChannelInvitesCacheKey(in Snowflake channelID) => $"Channel:{channelID}:Invites";

    public static string CreateChannelPermissionCacheKey(in Snowflake channelID, in Snowflake overwriteID) => $"Channel:{channelID}:Overwrite:{overwriteID}";
    
    public static string CreateCurrentUserCacheKey() => "User:@me";
    
    public static string CreateEmojiCacheKey(in Snowflake guildID, in Snowflake emojiID) => $"Guild:{guildID}:Emoji:{emojiID}";
    
    public static string CreateGuildCacheKey(in Snowflake guildID) => $"Guild:{guildID}";
    
    public static string CreateGuildChannelsCacheKey(in Snowflake guildID) => $"Guild:{guildID}:Channels";
    
    public static string CreateGuildEmojisCacheKey(in Snowflake guildID) => $"Guild:{guildID}:Emojis";
    
    public static string CreateGuildMemberCacheKey(in Snowflake guildID, in Snowflake userID) => $"Guild:{guildID}:Member:{userID}";
    
    public static string CreateGuildMembersCacheKey(in Snowflake guildID) => $"Guild:{guildID}:Members";
    
    public static string CreateGuildRoleCacheKey(in Snowflake guildID, in Snowflake roleID) => $"Guild:{guildID}:Role:{roleID}";
    
    public static string CreateGuildRolesCacheKey(in Snowflake guildID) => $"Guild:{guildID}:Roles";
    
    public static string CreateGuildVoiceStatesCacheKey(in Snowflake guildID) => $"Guild:{guildID}:VoiceStates";
    
    public static string CreateInviteCacheKey(string code) => $"Invite:{code}";
    
    public static string CreateMessageCacheKey(in Snowflake channelID, in Snowflake messageID) => $"Channel:{channelID}:Message:{messageID}";
    
    public static string CreatePresenceCacheKey(in Snowflake guildID, in Snowflake userID) => $"Guilds:{guildID}:Member:{userID}:Presence";
    
    public static string CreateThreadMemberCacheKey(in Snowflake threadID, in Snowflake userID) => $"Thread:{threadID}:Member:{userID}";
    
    public static string CreateThreadMembersCacheKey(in Snowflake threadID) => $"Thread:{threadID}:Members";
    
    public static string CreateUserCacheKey(in Snowflake userID) => $"User:{userID}";
}