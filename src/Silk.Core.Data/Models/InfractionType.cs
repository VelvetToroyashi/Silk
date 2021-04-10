namespace Silk.Core.Data.Models
{
    public enum InfractionType
    {
        /// <summary>
        ///     Used by Auto-Mod and the strike command. In the case of the former, the Infraction Handler
        ///     swill take appropriate action dependent on the number of strikes the user has.
        ///     When used by the warn command, after 5 strikes, Silk! will ask to elevate to the next appropriate action depending on the guild configuration.
        /// </summary>
        Warn,
        /// <summary>
        ///     Signifies the user was kicked when this infraction was added.
        /// </summary>
        Kick,
        /// <summary>
        ///     Signifies that the user was muted, either temporarily or indefinetly. These infractions are not held against the user.
        /// </summary>
        Mute,
        /// <summary>
        ///     Signifies the user was muted temporarily by auto-mod. These infractions are held against the user.
        /// </summary>
        AutoModMute,
        /// <summary>
        ///     Signifies the user was banned temporarily.
        /// </summary>
        SoftBan,
        /// <summary>
        ///     Signifies the user was banned indefinitely.
        /// </summary>
        Ban,
        /// <summary>
        ///     Used for <see cref="InfractionStep" />. This value signifies that no action will be taken against the user for this infraction.
        /// </summary>
        Ignore
    }
}