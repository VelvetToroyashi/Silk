using System;
using System.ComponentModel.DataAnnotations;

namespace AnnoucementPlugin.Database
{
    public sealed class AnnouncementModel
    {
        [Key]
        public int PK_Key { get; set; }

        public ulong GuildId   { get; set; }
        public ulong ChannelId { get; set; }

        [MaxLength(4000)]
        public string AnnouncementMessage { get; set; }

        public DateTime ScheduledFor { get; set; }
    }
}