using System.ComponentModel.DataAnnotations.Schema;
using Remora.Rest.Core;

namespace Silk.Data.Entities;

/// <summary>
/// Represents an unfulfilled greeting.
/// </summary>
[Table("pending_greetings")]
public class PendingGreetingEntity
{
    /// <summary>
    /// The ID of this greeting entity.
    /// </summary>
    [Column("id")]
    public int Id { get; set; }
    
    /// <summary>
    /// The ID of the guild the greeting is yet to be fulfilled for.
    /// </summary>
    [Column("guild_id")]
    public Snowflake GuildID { get; set; }
    
    /// <summary>
    /// The ID of the user to greet.
    /// </summary>
    [Column("user_id")]
    public Snowflake UserID { get; set; }
    
    /// <summary>
    /// The ID of the greeting to be fulfilled.
    /// </summary>
    [Column("greeting_id")]
    public int GreetingID  { get; set; }
}