using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SilkBot.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace SilkBot.Models
{
    public class DiscordUserInfo
    {
        [Key]
        public int Id { get; set; }
        
        public int Cash { get; set; }
        public Guild Guild { get; set; }
        public ulong UserId { get; set; }
        public UserFlag Flags { get; set; }
        public DateTime LastCashIn { get; set; }
        //public UserInfractionModel Infractions { get; set; }

    }
}
