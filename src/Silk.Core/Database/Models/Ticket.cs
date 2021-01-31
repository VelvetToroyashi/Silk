using System;
using System.Collections.Generic;

namespace Silk.Core.Database.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public bool IsOpen { get; set; }
        public ulong Opener { get; set; }
        public DateTime Opened { get; set; }
        public DateTime Closed { get; set; }
        public List<TicketMessage> History { get; set; } = new();
    }
}