using System;
using System.Collections.Generic;

namespace SilkBot.Database.Models
{
    public class TicketModel
    {
        public int Id { get; set; }
        public bool IsOpen { get; set; }
        public ulong Opener { get; set; }
        public DateTime Opened { get; set; }
        public DateTime Closed { get; set; }
        public IEnumerable<TicketResponderModel> Responders { get; set; } = new List<TicketResponderModel>();
        public List<TicketMessageHistoryModel> History { get; set; } = new List<TicketMessageHistoryModel>();
    }
}
