using SilkBot.Database.Models;

namespace SilkBot.Commands.General.Tickets
{
    public record TicketCreationResult(bool Succeeded, string? Reason, TicketModel? Ticket);
}