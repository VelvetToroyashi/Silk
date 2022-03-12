using Remora.Discord.API.Objects;
using Remora.Rest.Core;

namespace Silk.Remora.RedisCache;

public static class RedisKeyHelper
{
    public static string CreateChannelCacheKey(in Snowflake channelID) => $"Channel:{channelID}";
    
    public static string CreateGuildCacheKey(in Snowflake guildID) => $"Guild:{guildID}";
    
    public static string CreateUserCacheKey(in Snowflake userID) => $"User:{userID}";
    
    public static string CreateGuildRoleCacheKey(in Snowflake guildID, in Snowflake roleID) => $"Guild:{guildID}:Role:{roleID}";
    
    public static string CreateEmojiCacheKey(in Snowflake guildID, in Snowflake emojiID) => $"Guild:{guildID}:Emoji:{emojiID}";
    
    public static string CreateGuildMemberCacheKey(in Snowflake guildID, in Snowflake userID) => $"GuildMember:{guildID}:{userID}";
    
    public static string CreateGuildRolesCacheKey(in Snowflake guildID) => $"Guild:{guildID}:Roles";
   
    public static string CreateGuildMembersCacheKey(in Snowflake guildID) => $"Guild:{guildID}:Members";
    
    public static string CreateGuildEmojisCacheKey(in Snowflake guildID) => $"Guild:{guildID}:Emojis";
    
    public static string CreateGuildChannelsCacheKey(in Snowflake guildID) => $"Guild:{guildID}:Channels";
    
    public static string CreateGuildVoiceStatesCacheKey(in Snowflake guildID) => $"Guild:{guildID}:VoiceStates";
    
    public static string CreatePresenceCacheKey(in Snowflake guildID, in Snowflake userID) => $"Guilds:{guildID}:Presences:{userID}";

    public static string CreateCurrentUserCacheKey() => "User:@me";
    
    public static string CreateThreadMemberCacheKey(in Snowflake threadID, in Snowflake userID) => $"Thread:{threadID}:Members:{userID}";
    
    public static string CreateMessageCacheKey(in Snowflake channelID, in Snowflake messageID) => $"Channel:{channelID}:Message:{messageID}";
}