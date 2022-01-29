using System;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Rest.Core;

namespace Silk.Extensions.Remora;

public static class SnowflakeEntityExtensions
{
    #region Mentions

    /// <summary>
    /// An enum for mention types.
    /// </summary>
    private enum MentionType
    {
        User,
        Role,
        Channel
    }
    
    /// <summary>
    /// Turns a user into a mention.
    /// </summary>
    /// <param name="user">The user to mention.</param>
    /// <returns>The formatted mention. (e.g. &lt;@1234567890&gt; )</returns>
    public static string Mention(this IUser user)
        => Mention(user.ID, MentionType.User);

    /// <summary>
    /// Turns a member into a mention.
    /// </summary>
    /// <param name="member">The member to mention.</param>
    /// <returns>The formatted mention. (e.g. &lt;@1234567890&gt; )</returns>
    public static string Mention(this IGuildMember member)
        => member.User.IsDefined(out var user) 
            ? Mention(user.ID, MentionType.User) 
            : throw new InvalidOperationException("The member object does not contain a user.");

    /// <summary>
    /// Turns a role into a mention.
    /// </summary>
    /// <param name="user">The role to mention.</param>
    /// <returns>The formatted mention. (e.g. &lt;@&1234567890&gt; )</returns>
    public static string Mention(this IRole role)
        => Mention(role.ID, MentionType.Role);
    
    /// <summary>
    /// Turns a channel into a mention.
    /// </summary>
    /// <param name="channel">The channel to mention</param>
    /// <returns>The formatted mention. (e.g. &lt;#1234567890&gt; )</returns>
    public static string Mention(this IChannel channel)
        => Mention(channel.ID, MentionType.Channel);

    private static string Mention(Snowflake id, MentionType type)
        => type switch
        {
            MentionType.Channel => $"<#{id}>",
            MentionType.Role    => $"<@&{id}>",
            MentionType.User    => $"<@{id}>",
            _                   => throw new ArgumentOutOfRangeException(nameof(type), type, "Mention type must be user, role, or channel.")
        };

    #endregion
    
    /// <summary>
    /// Formats a user into their Discord tag (their username with discriminator).
    /// </summary>
    /// <param name="user">The user to format.</param>
    /// <returns>The user's tag. (e.g. Velvet#0069)</returns>
    public static string ToDiscordTag(this IUser user)
        => $"{user.Username}#{user.Discriminator:0000}";
    
    /// <summary>
    /// Formats a member into their Discord tag (their username with discriminator).
    /// </summary>
    /// <param name="member">The member to format.</param>
    /// <returns>The member's tag. (e.g. Velvet#0069)</returns>
    public static string ToDiscordTag(this IGuildMember member)
        => member.User.IsDefined(out var user) 
            ? user.ToDiscordTag() 
            : throw new InvalidOperationException("The member object does not contain a user.");
}