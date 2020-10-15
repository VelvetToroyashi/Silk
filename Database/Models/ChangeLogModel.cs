using System;
using System.Collections.Generic;
using System.Text;

namespace SilkBot.Database.Models
{
    public class ChangelogModel
    {
        public int Id { get; set; }
        public string Authors { get; set; }
        public string Version { get; set; }
        public string Additions { get; set; }
        public string Removals { get; set; }
        public DateTime ChangeTime { get; set; }
    }
}
