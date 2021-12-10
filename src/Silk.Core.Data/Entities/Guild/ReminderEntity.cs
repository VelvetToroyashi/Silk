using System;
using System.ComponentModel.DataAnnotations.Schema;
using Remora.Rest.Core;

namespace Silk.Core.Data.Entities;

/// <summary>
///     A reminder that is sent to a user at a later time.
/// </summary>
public class ReminderEntity
{
    public int Id { get; set; }

    /// <summary>
    ///     When this reminder expires.
    /// </summary>
    [Column("expires_at")]
    public DateTimeOffset ExpiresAt { get; set; }

    /// <summary>
    ///     When this reminder was created.
    /// </summary>
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    ///     The Id of the owner.
    /// </summary>
    [Column("owner_id")]
    public Snowflake OwnerID { get; init; }

    /// <summary>
    ///     The channel the reminder was made in
    /// </summary>
    [Column("channel_id")]
    public Snowflake ChannelID { get; set; }

    /// <summary>
    ///     The guild this reminder was created in.
    /// </summary>
    [Column("guild_id")]
    public Snowflake? GuildID { get; set; }

    /// <summary>
    ///     The Id of the message to remind them of
    /// </summary>
    [Column("message_id")]
    public Snowflake? MessageID { get; set; }

    /// <summary>
    ///     The content of the original reminder, in case a message can't be sent to the original channel.
    /// </summary>
    [Column("content")]
    public string? MessageContent { get; set; }

    /// <summary>
    ///     The content of the message the reply contained, if the reminder was a reply.
    /// </summary>
    [Column("reply_content")]
    public string? ReplyMessageContent { get; set; }

    /// <summary>
    ///     The Id of the author of the reply the reminder was set with, if any.
    /// </summary>
    [Column("reply_author_id")]
    public Snowflake? ReplyAuthorID { get; set; }

    /// <summary>
    ///     The Id of the message that was replied to, if any.
    /// </summary>
    [Column("reply_message_id")]
    public Snowflake? ReplyID { get; set; }
}