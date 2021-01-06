using System.Threading.Tasks;
using DSharpPlus.Entities;
using Silk.Core.Database.Models;

namespace Silk.Core.Services.Interfaces
{
    /// <summary>
    /// Interface for any service that serves as the gatekeeper for punishing users, and applying infractions to users.
    /// </summary>
    public interface IInfractionService
    {
        /// <summary>
        /// Checks whether a member is exempt from their message being deleted by Auto-Mod.
        /// </summary>
        /// <param name="member">The member to check.</param>
        public Task<bool> ShouldDeleteMessageAsync(DiscordMember member);
        /// <summary>
        /// Adds an infraction to the queue to be processed.
        /// </summary>
        /// <param name="member">The member this infraction belongs to.</param>
        /// <param name="infraction">Their infraction.</param>
        public void AddInfraction(DiscordMember member, UserInfractionModel infraction);
        
        
    }
}