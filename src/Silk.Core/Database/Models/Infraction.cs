using System;

namespace Silk.Core.Database.Models
{
    public class
        Infraction
    {
        public int Id { get; set; } //Requisite Id for DB purposes
        /// <summary>
        /// The Id of the user this infraction belongs to.
        /// </summary>
        public ulong UserId { get; set; }
        /// <summary>
        /// The reason this infraction was given. Infractions added by Auto-Mod will be prefixed with "[AUTO-MOD]".
        /// </summary>
        public string Reason { get; set; } // Why was this infraction given
        /// <summary>
        /// The Id of the user that gave this infraction; Auto-Mod infractions will default to the bot.
        /// </summary>
        public ulong Enforcer { get; set; } //Who gave this infraction
        /// <summary>
        /// The user object this infraction belongs to, to form the Foreign Key (FK).
        /// </summary>
        public User User { get; set; } //Who's this affecting
        /// <summary>
        /// The time this infraction was added.
        /// </summary>
        /// <remarks>This is also used for the infraction service to check</remarks>
        public DateTime InfractionTime { get; set; } //When it happened
        /// <summary>
        /// The type of infraction. Infractions of type <see cref="Silk.Core.Database.Models.InfractionType.SoftBan"/> and <see cref="Silk.Core.Database.Models.InfractionType.Mute"/> are loaded into memory upon startup.
        /// </summary>
        public InfractionType InfractionType { get; set; } //What happened

        /// <summary>
        /// Whether or not this infraction will be held against the user in question; this is used by the <see cref="Silk.Core.Services.Interfaces.IInfractionService"/>
        /// to determine whether or not to take a higher action.
        /// </summary>
        public bool HeldAgainstUser { get; set; } // Used for infraction service to determine whether to escalate or not //

        public DateTime? Expiration { get; set; }
    }
}