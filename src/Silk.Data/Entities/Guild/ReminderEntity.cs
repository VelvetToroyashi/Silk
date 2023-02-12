using System;
using Remora.Rest.Core;

namespace Silk.Data.Entities;

/// <summary>
///     A reminder that is sent to a user at a later time.
/// </summary>
public class ReminderEntity
{
    public int Id { get; set; }

    /// <summary>
    ///     When this reminder expires.
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; }

    /// <summary>
    ///     When this reminder was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Whether this reminder is private. Reminders sent in DMs or invoked by slash commands are private.
    /// </summary>
    public bool IsPrivate { get; set; }
    
    /// <summary>
    /// Whether this reminder should be sent with `SUPPRESS_NOTIFICATIONS` flag set on the message
    /// </summary>
    public bool IsQuiet { get; set; }
    
    /// <summary>
    /// Whether the reminder message was a reply to another message.
    /// </summary>
    public bool IsReply { get; set; }
    
    /// <summary>
    ///     The Id of the owner.
    /// </summary>
    public Snowflake OwnerID { get; init; }

    /// <summary>
    ///     The channel the reminder was made in
    /// </summary>
    public Snowflake ChannelID { get; set; }

    /// <summary>
    ///     The guild this reminder was created in.
    /// </summary>
    public Snowflake? GuildID { get; set; }

    /// <summary>
    ///     The Id of the message to remind them of
    /// </summary>
    public Snowflake? MessageID { get; set; }

    /// <summary>
    ///     The content of the original reminder, in case a message can't be sent to the original channel.
    /// </summary>
    public string? MessageContent { get; set; }

    /// <summary>
    ///     The content of the message the reply contained, if the reminder was a reply.
    /// </summary>
    public string? ReplyMessageContent { get; set; }

    /// <summary>
    ///     The Id of the author of the reply the reminder was set with, if any.
    /// </summary>
    public Snowflake? ReplyAuthorID { get; set; }

    /// <summary>
    ///     The Id of the message that was replied to, if any.
    /// </summary>
    public Snowflake? ReplyMessageID { get; set; }
}