using System;
using System.Collections.Generic;

namespace Silk.Core.Data.Entities
{
    public class TagEntity
    {
        public int Id { get; set; }
        public int Uses { get; set; }

        public string Name { get; set; }
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }

        public ulong OwnerId { get; set; }
        public ulong GuildId { get; set; }

        public int? OriginalTagId { get; set; }
        public TagEntity? OriginalTag { get; set; }

        public List<TagEntity>? Aliases { get; set; }
    }
}