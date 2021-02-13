using Silk.Core.Database.Models;

namespace Silk.Core.Commands.General.Tickets
{
    public record TicketCreationResult(bool Succeeded, string? Reason, Ticket? Ticket);
}