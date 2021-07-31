namespace Silk.Core.Data.Models
{
    public class ReactionRole
    {
        /// <summary>
        ///     The Id of the reaction role.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     The Id of the emoji related to the reaction role.
        /// </summary>
        public string EmojiName { get; set; }

        /// <summary>
        ///     The Id of the role to assign when receiving a reaction.
        /// </summary>
        public ulong RoleId { get; set; }

        /// <summary>
        ///     The Id of the message this reaction role belongs to.
        /// </summary>
        public ulong MessageId { get; set; }

        /// <summary>
        ///     The Id of the Guild configuration this reaction role belongs to.
        /// </summary>
        public int GuildConfigId { get; set; }

        //public ReactionRole? RequiredRole { get; set; }
        //public List<ReactionRole>? InvalidRoles { get; set; }
    }
}