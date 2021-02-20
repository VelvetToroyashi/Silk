using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Silk.Data.Models
{
    [Keyless]
    public class Command
    {
        public string Name { get; set; } = "";
        public string Usage { get; set; } = "";
        public string[] Parameters { get; set; } = null!;
        public string? Description { get; set; }
        public string? Notes { get; set; } = "";
        public bool IsStaffOnly { get; set; }
        public Command[]? Overloads { get; set; } = null!;
        public List<Command>? SubCommands { get; set; }
    }
}