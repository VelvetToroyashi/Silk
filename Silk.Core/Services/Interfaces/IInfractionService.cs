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
        public Task KickAsync(DiscordMember member, DiscordChannel channel, UserInfractionModel infraction);
        public Task BanAsync(DiscordMember member, DiscordChannel channel, UserInfractionModel infraction);
        public Task TempBanAsync(DiscordMember member, DiscordChannel channel, UserInfractionModel infraction);
        public Task MuteAsync(DiscordMember member, DiscordChannel channel, UserInfractionModel infraction);
        public Task<UserInfractionModel> CreateInfractionAsync(DiscordMember member, DiscordMember enforcer, InfractionType type, string reason = "Not given.");
        public Task<UserInfractionModel> CreateTemporaryInfractionAsync(DiscordMember member, DiscordMember enforcer, InfractionType type, string reason = "Not given.", DateTime? expiration = null);
        public Task<bool> ShouldAddInfractionAsync(DiscordMember member);
        public Task<bool> HasActiveMuteAsync(DiscordMember member);
    }
}