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
        public ulong GuildId { get; set; }
        
        /// <summary>
        /// The ID of the user to greet.
        /// </summary>
        public ulong UserId  { get; set; }
    }
}