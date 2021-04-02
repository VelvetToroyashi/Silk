using Silk.Core.Data.Models;

namespace Silk.Core.Discord.Commands.General.Tickets
{
    public record TicketCreationResult(bool Succeeded, string? Reason, Ticket? Ticket);
}