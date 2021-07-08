using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Silk.Core.Data.DTOs;
using Silk.Core.Data.Models;
using Silk.Core.Types;

namespace Silk.Core.Services.Interfaces
{
	/// <summary>
	/// A robust set of moderation-related methods for handling and enforcing infractions.
	/// </summary>
	public interface IInfractionService
	{
		/// <summary>
		/// Kicks a member from the specified guild.
		/// </summary>
		/// <param name="userId">The id of the user to remove.</param>
		/// <param name="guildId">The id of the guild to remove the user from.</param>
		/// <param name="enforcerId">The id of the member that removed the specified user.</param>
		/// <param name="reason">The reason the user is being removed.</param>
		/// <returns>A value indicating the resulting state of the operation.</returns>
		public Task<InfractionResult> KickAsync(ulong userId, ulong guildId, ulong enforcerId, string reason = "Not Given.");
		
		/// <summary>
		///  Bans a memebr from the specified guild, eithe rpermanently or temporarily.
		/// </summary>
		/// <param name="userId">The id of the user to ban.</param>
		/// <param name="guildId">The id of the guild to ban the user on.</param>
		/// <param name="enforcerId">The id fo the member that is banning the specified user</param>
		/// <param name="reason">The reason the user is being banned.</param>
		/// <param name="expiration">If the user is beined temp-banned, the date the user should be unbanned.</param>
		/// <returns>A value indicating the resulting state of the operation.</returns>
		public Task<InfractionResult> BanAsync(ulong userId, ulong guildId, ulong enforcerId, string reason = "Not Given.", DateTime? expiration = null);
		
		/// <summary>
		/// Un-bans a member from the specified guild.
		/// </summary>
		/// <param name="userId">The id of the user to unban.</param>
		/// <param name="guildId">THe id of the guild to unban the user from.</param>
		/// <param name="enforcerId">The id of the member that is unbanning the user.</param>
		/// <param name="reason">The reason the user is being unbanned.</param>
		/// <returns>A value indicating the resulting state of the operation.</returns>
		public Task<InfractionResult> UnBanAsync(ulong userId, ulong guildId, ulong enforcerId, string reason = "Not Given.");
		
		/// <summary>
		/// Warns/"strikes" a user on the specified guild.
		/// </summary>
		/// <param name="userId">The id of the user being striked.</param>
		/// <param name="guildId">The id of the guild the user is being striked on.</param>
		/// <param name="enforcerId">The id of the member that is striking the user.</param>
		/// <param name="reason">The reason the user is being striked.</param>
		/// <param name="autoEscalate">Whether or not the strike should be automatically escalated.</param>
		/// <returns>A value indicating the resulting state of the operation.</returns>
		public Task<InfractionResult> StrikeAsync(ulong userId, ulong guildId, ulong enforcerId, string reason = "Not Given.", bool autoEscalate = false);
		
		/// <summary>
		/// Gets whether a user has an active mute on the specified guild.
		///
		/// <remarks>
		///	This method's signature is <see cref="ValueTask{TResult}"/> in contrast to the rest being <see cref="Task{TResult}"/>
		/// because in typical use-case it is expected that infractions are cached, and can provide a sync path via in-memory lookup.
		/// </remarks>
		/// </summary>
		/// <param name="userId">The id of the user to check.</param>
		/// <param name="guildId">The id of the guild to check.</param>
		/// <returns>Whether the user is currently muted.</returns>
		public ValueTask<bool> IsMutedAsync(ulong userId, ulong guildId);
		
		/// <summary>
		/// Mutes a user on the guild, either temporarily or permanently.
		/// </summary>
		/// <param name="userId">The id of the user to mute.</param>
		/// <param name="guildId">The id of the guild to mute the user on.</param>
		/// <param name="enforcerId">The member that's muting the user.</param>
		/// <param name="reason">The reason the user is beign muted.</param>
		/// <param name="expiration">If temporarily muting, when this mute is set to expire.</param>
		/// <returns>A value indicating the resulting state of the operation.</returns>
		public Task<InfractionResult> MuteAsync(ulong userId, ulong guildId, ulong enforcerId, string reason = "Not Given.", DateTime? expiration = null);
		
		/// <summary>
		/// Un-mutes a member on the specified guild.
		/// </summary>
		/// <param name="userId">The id of the user to unmute.</param>
		/// <param name="guildId">The id of the guild to unmute the user on.</param>
		/// <param name="enforcerId">The member that's unmuting the user.</param>
		/// <param name="reason">The reason teh member was unmuted.</param>
		/// <returns>A value indicating the resulting state of the operation.</returns>
		public Task<InfractionResult> UnMuteAsync(ulong userId, ulong guildId, ulong enforcerId, string reason = "Not Given.");
		
		/// <summary>
		/// Gets the currently configured infraction step. Primarily used for auto-mod.
		/// </summary>
		/// <param name="guildId">The id of the guild to get the current infraction step for.</param>
		/// <param name="infractions">The infractions to check. Only non-rescinded infractions (excluding notes) count toward the current infraction step.</param>
		/// <returns>A value indicating the resulting state of the operation.</returns>
		public Task<InfractionStep> GetCurrentInfractionStepAsync(ulong guildId, IEnumerable<InfractionDTO> infractions);
		
		/// <summary>
		/// Generates an infraction for the specified user.
		///
		/// <remarks>
		///	This method is utilized in interal APIs and is not meant for direct use. 
		/// <br/>
		/// This method is subject to change and should not be used outside of API wrappers.
		/// </remarks>
		/// </summary>
		/// <param name="userId">The id of the user to generate an infraction for.</param>
		/// <param name="guildId">The id of the guild to generate an infraction for.</param>
		/// <param name="enforcerId">The id of the member that generated this infraction.</param>
		/// <param name="type">The type of infraction being generated.</param>
		/// <param name="reason">The reason this infraction is being generated.</param>
		/// <param name="expiration">When this infraction expires, if ever, if applicable.</param>
		/// <returns>A value indicating the resulting state of the operation.</returns>
		public Task<InfractionDTO> GenerateInfractionAsync(ulong userId, ulong guildId, ulong enforcerId, InfractionType type, string reason = "Not Given.", DateTime? expiration = null);
		
		
		/// <summary>
		/// Adds a note to the specified user. Notes do not count toward automatic infraction-escalation.
		/// </summary>
		/// <param name="userId">The id of the user to add a note to .</param>
		/// <param name="guildId">The id of the guild the user is on.</param>
		/// <param name="noterId">The if of the member adding the note to the user.</param>
		/// <param name="note">The note to add.</param>
		/// <returns>A value indicating the resulting state of the operation.</returns>
		public Task<InfractionResult> AddNoteAsync(ulong userId, ulong guildId, ulong noterId, string note);

		
		/// <summary>
		/// Pardons a user from their most recent infraction (strike or escalated).
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="guildId"></param>
		/// <param name="enforcerId"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public Task<InfractionResult> PardonAsync(ulong userId, ulong guildId, ulong enforcerId, string reason = "Not Given.");
	}
}