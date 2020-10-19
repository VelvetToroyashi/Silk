using System.ComponentModel.DataAnnotations;

namespace SilkBot.Database.Models
{
    public class TicketResponderModel
    {
        public ulong ResponderId { get; set; }
        public string Name { get; set; }
    }
}
