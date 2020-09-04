using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

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
