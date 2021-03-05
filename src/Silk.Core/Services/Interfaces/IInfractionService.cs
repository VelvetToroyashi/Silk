using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Data.Models;

namespace Silk.Core.Services.Interfaces
{
    /// <summary>
    /// Gatekeeper for infractions and semantic wrapper for handling moderation actions.
    /// </summary>
    public interface IInfractionService
    {
        /// <summary>
        /// Kick a member from the guild.
        /// </summary>
        /// <param name="member">The member to kick.</param>
        /// <param name="channel">The channel the command was executed in.</param>
        /// <param name="infraction">The infraction generated for the user.</param>
        /// <param name="embed">The embed to log to the appropriate log channel, if configured.</param>
        public Task KickAsync(DiscordMember member, DiscordChannel channel, Infraction infraction, DiscordEmbed embed);

        /// <summary>
        /// Bans a member indefinitely.
        /// </summary>
        /// <param name="member">The member to ban.</param>
        /// <param name="channel">The channel the command was executed in.</param>
        /// <param name="infraction">The infraction generated for the user.</param>
        public Task BanAsync(DiscordMember member, DiscordChannel channel, Infraction infraction);
        /// <summary>
        /// Temporarily bans a member from a guild. Tempbans cannot exceed one year (365 from the time of the infraction) in length.
        /// </summary>
        /// <param name="member">The member being banned.</param>
        /// <param name="channel">The channel the command was executed in.</param>
        /// <param name="infraction">The infraction generated for the user.</param>
        /// <param name="embed">The message to log to the appropriate logging channel, if configured.</param>
        public Task TempBanAsync(DiscordMember member, DiscordChannel channel, Infraction infraction, DiscordEmbed embed);
        
        /// <summary>
        /// Mutes a member indefinitely, or temporarily.
        /// </summary>
        /// <param name="member">The member to mute.</param>
        /// <param name="channel">The channel the command was executed in.</param>
        /// <param name="infraction">The infraction generated for the user.</param>
        public Task MuteAsync(DiscordMember member, DiscordChannel channel, Infraction infraction);
        
        /// <summary>
        /// Automatically determines the next course of action given the user's infraction count.
        /// </summary>
        /// <param name="member">The member the infraction belongs to, if escalation is required.</param>
        /// <param name="infraction">The infraction to pass, if escelation is required.</param>
        public Task ProgressInfractionStepAsync(DiscordMember member, Infraction infraction);
        
        /// <summary>
        /// Creates an <see cref="Infraction"/> object to pass to moderation methods.
        /// </summary>
        /// <param name="member">The member to generate the infraction for.</param>
        /// <param name="enforcer">The member adminstering this infraction.</param>
        /// <param name="type">The type of infraction.</param>
        /// <param name="reason">The reason the infraction was generated.</param>
        /// <returns>The generated <see cref="Infraction"/>.</returns>
        public Task<Infraction> CreateInfractionAsync(DiscordMember member, DiscordMember enforcer, InfractionType type, string reason = "Not given.");
        
        /// <summary>
        /// Creates a temporary <see cref="Infraction"/>.
        /// </summary>
        /// <param name="member">The member to generate the infraction for.</param>
        /// <param name="enforcer">The member adminstering this infraction.</param>
        /// <param name="type">The type of infraction.</param>
        /// <param name="reason">The reason the infraction was generated.</param>
        /// <param name="expiration">The time the infraction expires, relative to now.</param>
        /// <returns>The genrated <see cref="Infraction"/>.</returns>
        public Task<Infraction> CreateTempInfractionAsync(DiscordMember member, DiscordMember enforcer, InfractionType type, string reason = "Not given.", DateTime? expiration = null);
        
        /// <summary>
        /// Returns whether action should be taken against a member.
        /// </summary>
        /// <param name="member">The member to check.</param>
        /// <returns>True if the user does not have the <see cref="UserFlag.InfractionExemption"/> flag, else false.</returns>
        public Task<bool> ShouldAddInfractionAsync(DiscordMember member);
        
        /// <summary>
        /// Returns whether a given member is currently muted.
        /// </summary>
        /// <param name="member">The member to check.</param>
        public Task<bool> HasActiveMuteAsync(DiscordMember member);
    }
}