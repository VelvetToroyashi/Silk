using System.ComponentModel.DataAnnotations;

namespace SilkBot.Database.Models
{
    public class TicketMessageHistoryModel
    {
        [Key]
        public ulong Sender { get; set; }
        public string Message { get; set; }
        public TicketModel TicketModel { get; set; }
    }
}