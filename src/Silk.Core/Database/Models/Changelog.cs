using System;

namespace Silk.Core.Database.Models
{
    public class Changelog
    {
        public int Id { get; set; }
        public string Authors { get; set; }
        public string Version { get; set; }
        public string Additions { get; set; }
        public string Removals { get; set; }
        public DateTime ChangeTime { get; set; }
    }
}