using System.Threading.Tasks;
using DSharpPlus.Entities;
using Silk.Core.Database.Models;
using Silk.Core.Services.Tickets;

namespace Silk.Core.Services.Interfaces
{
    public interface ITicketService
    {
        /// <summary>
        /// Create a TicketModel in the database, and add the message sent as the first piece of history for it. 
        /// </summary>
        /// <param name="user">The user that opened the ticket.</param>
        /// <param name="message">The message they sent.</param>
        /// <returns>Result of creating the ticket, indicating success or failure.</returns>
        Task<TicketCreationResult> CreateTicketAsync(DiscordUser user, string message);

        Task CloseTicketByUserIdAsync(ulong userId);
        Task CloseTicketByChannelAsync(DiscordMessage message);
        
        Task<Ticket> GetTicketByUserIdAsync(ulong userId);

        Task<ulong> GetUserIdByChannel(DiscordChannel channel);
        Task<ulong> GetTicketChannelByUserId(ulong userId);
        
        Task<bool> UserHasOpenTicketAsync(ulong userId);
    }
}