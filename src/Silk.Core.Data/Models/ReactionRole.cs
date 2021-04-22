namespace Silk.Core.Data.Models
{
    public class ReactionRole
    {
        /// <summary>
        /// The role Id of the reaction role.
        /// </summary>
        public ulong Id { get; set; }

        /// <summary>
        /// The Id of the emoji related to the reaction role.
        /// </summary>
        /// 
        public ulong EmojiId { get; set; }

        /// <summary>
        /// The Id of the message this reaction role belongs to.
        /// </summary>
        public ulong MessageId { get; set; }

        /// <summary>
        /// The Id of the Guild configuration this reaction role belongs to.
        /// </summary>
        public int GuildConfigId { get; set; }

        //public ReactionRole? RequiredRole { get; set; }
        //public List<ReactionRole>? InvalidRoles { get; set; }
    }
}