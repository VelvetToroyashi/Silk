namespace Silk.Core.Data.Entities
{
    /// <summary>
    /// Represents a greeting that has yet to be fulfilled.
    /// </summary>
    public class PendingMemberGreetingEntity
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
        /// The ID of the channel to greet in.
        /// </summary>
        public ulong ChannelId { get; set; }

        /// <summary>
        /// The ID of the user to greet.
        /// </summary>
        public ulong UserId  { get; set; }
        
        /// <summary>
        /// See <see cref="GuildGreetingEntity.MetadataSnowflake"/>.
        /// </summary>
        public ulong MetadataId { get; set; }
        
        /// <summary>
        /// The option to greet on.
        /// </summary>
        public GreetingOption GreetingOption { get; set; }
    }
}