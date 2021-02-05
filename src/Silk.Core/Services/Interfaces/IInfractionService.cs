using System;
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
        /// Kick a member verbosely, by logging to the appropriate log channel (if any), and the chanel the command was run in.
        /// </summary>
        public Task VerboseKickAsync(DiscordMember member, DiscordChannel channel, Infraction infraction, DiscordEmbed embed);
        
        public Task BanAsync(DiscordMember member, DiscordChannel channel, Infraction infraction);
        public Task TempBanAsync(DiscordMember member, DiscordChannel channel, Infraction infraction);
        public Task MuteAsync(DiscordMember member, DiscordChannel channel, Infraction infraction);
        /// <summary>
        /// Automatically determines the next course of action given the user's current amount of 
        /// </summary>
        /// <param name="member"></param>
        /// <param name="infraction"></param>
        /// <returns></returns>
        public Task ProgressInfractionStepAsync(DiscordMember member, Infraction infraction);
        public Task<Infraction> CreateInfractionAsync(DiscordMember member, DiscordMember enforcer, InfractionType type, string reason = "Not given.");
        public Task<Infraction> CreateTemporaryInfractionAsync(DiscordMember member, DiscordMember enforcer, InfractionType type, string reason = "Not given.", DateTime? expiration = null);
        public Task<bool> ShouldAddInfractionAsync(DiscordMember member);
        public Task<bool> HasActiveMuteAsync(DiscordMember member);
    }
}