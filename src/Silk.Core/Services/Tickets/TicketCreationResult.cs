using Silk.Core.Database.Models;

namespace Silk.Core.Services.Tickets
{
    public record TicketCreationResult(bool Succeeded, string? Reason, Ticket? Ticket);
}