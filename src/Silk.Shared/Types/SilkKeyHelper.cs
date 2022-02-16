using Microsoft.Extensions.Caching.Memory;
using Remora.Rest.Core;

namespace Silk.Shared.Types;

/// <summary>
///     <para>A helper class for generating keys to store in an <see cref="IMemoryCache" /></para>
///     <para>These keys are formatted in a way that should be compatible with Redis, too.</para>
/// </summary>
public static class SilkKeyHelper
{
    /// <summary>
    ///     Generates a key for a regular guild config.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <returns>The generated key.</returns>
    public static object GenerateGuildKey(Snowflake guildId)
        => $"guild_config:{guildId}";

    /// <summary>
    ///     Generates a key for a moderation guild config.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <returns>The generated key.</returns>
    public static object GenerateGuildModKey(Snowflake guildId)
        => $"guild_mod_config:{guildId}";

    /// <summary>
    ///     Generates a key for a guild prefix.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <returns>The generated key.</returns>
    public static object GenerateGuildPrefixKey(Snowflake guildId)
        => $"guild_prefix:{guildId}";
    
    public static object GenerateGuildMemberCountKey(Snowflake guildId)
        => $"guild_member_count:{guildId}";
    
    public static object GenerateGuildCountKey()
        => "guild_count";
    
    public static object GenerateCurrentGuildCounterKey()
        => "current_guild_count";
    
    public static object GenerateGuildIdentifierKey(Snowflake guildId)
        => $"guild_identifier:{guildId}";
    

}