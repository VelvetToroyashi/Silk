#region

using Silk.Core.Database.Models;

#endregion

namespace Silk.Core.Commands.General.Tickets
{
    public record TicketCreationResult(bool Succeeded, string? Reason, TicketModel? Ticket);
}