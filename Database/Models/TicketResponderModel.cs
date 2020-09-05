using System.ComponentModel.DataAnnotations;

namespace SilkBot.Database.Models
{
    public class TicketResponderModel
    {
        [Key]
        public ulong ResponderId { get; set; }
        public string Name { get; set; }
        public TicketModel Ticket { get; set; }
    }
}
