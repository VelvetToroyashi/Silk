using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Silk.Data.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public int Uses { get; set; }
        
        public string Name { get; set; }
        public string Content { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public ulong OwnerId { get; set; }
        public ulong GuildId { get; set; }
        
        public int? OriginalTagId { get; set; }
        public Tag? OriginalTag { get; set; }

        public List<Tag>? Aliases { get; set; }
    }
}