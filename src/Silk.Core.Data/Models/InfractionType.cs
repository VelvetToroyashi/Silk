namespace Silk.Core.Data.Models
{
    public enum InfractionType
    {
        /// <summary>
        ///     Used by Auto-Mod and the strike command. In the case of the former, the Infraction Handler
        ///     swill take appropriate action dependent on the number of strikes the user has.
        ///     When used by the warn command, after 5 strikes, Silk! will ask to elevate to the next appropriate action depending on the guild configuration.
        /// </summary>
        Strike,
        /// <summary>
        ///     Signifies the user was kicked when this infraction was added.
        /// </summary>
        Kick,
        /// <summary>
        /// A mute. Indefintite or temporary. 
        /// </summary>
        Mute,
        /// <summary>
        /// A mute given by the AutoMod system.
        /// </summary>
        AutoModMute,
        /// <summary>
        /// A temporary ban.
        /// </summary>
        SoftBan,
        /// <summary>
        /// An permenant ban.
        /// </summary>
        Ban,
        /// <summary>
        /// Treated as <see cref="Strike"/>, but it is not held against the user. 
        /// </summary>
        Ignore,
        /// <summary>
        /// The user was unmuted.
        /// </summary>
        Unmute
    }
}