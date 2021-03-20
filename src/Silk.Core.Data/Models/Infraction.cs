using System;

namespace Silk.Core.Data.Models
{
    public class Infraction
    {
        public int Id { get; set; } //Requisite Id for DB purposes
        /// <summary>
        /// The Id of the user this infraction belongs to.
        /// </summary>
        public ulong UserId { get; set; }
        
        /// <summary>
        /// The Id of the guild this infraction was given on.
        /// </summary>
        public ulong GuildId { get; set; }
        
        /// <summary>
        /// The guild this infraction was given on.
        /// </summary>
        public Guild Guild { get; set; }
        
        /// <summary>
        /// The reason this infraction was given. Infractions added by Auto-Mod will be prefixed with "[AUTO-MOD]".
        /// </summary>
        public string Reason { get; set; } // Why was this infraction given
        
        /// <summary>
        /// Whether this infraction has been processed.
        /// </summary>
        public bool Handled { get; set; }
        
        /// <summary>
        /// The Id of the user that gave this infraction; Auto-Mod infractions will default to the bot.
        /// </summary>
        public ulong Enforcer { get; set; } //Who gave this infraction
        
        ///// <summary>
        ///// The user object this infraction belongs to, to form the Foreign Key (FK).
        ///// </summary>
        //public User User { get; set; } //Who's this affecting
        
        /// <summary>
        /// The time this infraction was added.
        /// </summary>
        public DateTime InfractionTime { get; set; } //When it happened
        
        /// <summary>
        /// The type of infraction. Infractions of type <see cref="Models.InfractionType.SoftBan"/> and <see cref="Models.InfractionType.Mute"/> are loaded into memory upon startup.
        /// </summary>
        public InfractionType InfractionType { get; set; } //What happened

        /// <summary>
        /// Whether this is an active infraction and/or this infraction counts toward any auto-incrementing severity of infractions.
        /// Infraction will still hold on the user's record but is not held against them if set to false.
        /// </summary>
        public bool HeldAgainstUser { get; set; } // Used for infraction service to determine whether to escalate or not //

        /// <summary>
        /// When this infraction is set to expire. Resolves to null 
        /// </summary>
        public DateTime? Expiration { get; set; }
    }
}