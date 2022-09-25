using Remora.Rest.Core;

namespace Silk.Data.Entities;

/// <summary>
/// Represents an unfulfilled greeting.
/// </summary>
public class PendingGreetingEntity
{
    /// <summary>
    /// The ID of this greeting entity.
    /// </summary>
    public int ID { get; set; }
    
    /// <summary>
    /// The ID of the guild the greeting is yet to be fulfilled for.
    /// </summary>
    public Snowflake GuildID { get; set; }
    
    /// <summary>
    /// The ID of the user to greet.
    /// </summary>
    public Snowflake UserID { get; set; }
    
    /// <summary>
    /// The ID of the greeting to be fulfilled.
    /// </summary>
    public int GreetingID  { get; set; }
}