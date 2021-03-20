namespace Silk.Data.Models
{
    public class TicketMessage
    {
        public int Id { get; set; }
        public ulong Sender { get; set; }
        public string Message { get; set; }
        public Ticket Ticket { get; set; }
    }
}