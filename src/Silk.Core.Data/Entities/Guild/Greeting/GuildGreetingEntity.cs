using Remora.Rest.Core;

namespace Silk.Core.Data.Entities;

/// <summary>
///     Represents data for a greeting.
/// </summary>
public class GuildGreetingEntity
{
    /// <summary>
    ///     The ID of the greeting.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///     The id of the guild this greeting belongs to.
    /// </summary>
    public Snowflake GuildID { get; set; }

    /// <summary>
    ///     The message to greet the user with.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    ///     When to greet the user.
    /// </summary>
    public GreetingOption Option { get; set; }

    /// <summary>
    ///     The ID of the channel to send the greeting to.
    /// </summary>
    public Snowflake ChannelID { get; set; }

    /// <summary>
    ///     <para>The ID of the metadata to use for contextual greeting.</para>
    ///     <para>In the case of greeting when the user receives a role, this will be the ID of the role to check for before greeting</para>
    ///     <para>In the case of greeting when a user gains access to a new channel, this will be the ID of the channel to check before greeting.</para>
    /// </summary>
    public Snowflake? MetadataID { get; set; }
}