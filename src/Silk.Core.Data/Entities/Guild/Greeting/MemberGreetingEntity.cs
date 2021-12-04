using System.ComponentModel.DataAnnotations.Schema;

namespace Silk.Core.Data.Entities
{
    /// <summary>
    /// Represents a greeting that has yet to be fulfilled.
    /// </summary>
    public class MemberGreetingEntity
    {
        /// <summary>
        /// The ID of the pending greeting.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// The ID of the guild to greet on.
        /// </summary>
        [Column("guild")]
        public ulong GuildId { get; set; }
        
        /// <summary>
        /// The ID of the user to greet.
        /// </summary>
        [Column("user")]
        public ulong UserId  { get; set; }
        
        /// <summary>
        /// When to greet the user.
        /// </summary>
        [Column("when")]
        public GreetingOption Greeting { get; set; }
    }
}