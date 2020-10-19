using System.ComponentModel.DataAnnotations;

namespace SilkBot.Database.Models
{
    public class TicketMessageHistoryModel
    {
        public int Id { get; set; }
        public ulong Sender { get; set; }
        public string Message { get; set; }
        public TicketModel TicketModel { get; set; }
    }
}