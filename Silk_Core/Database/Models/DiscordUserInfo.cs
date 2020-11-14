using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SilkBot.Models
{
    public class UserModel
    {
        [Key]
        public ulong Id { get; set; }
        [ForeignKey("Guild")]
        public GuildModel Guild { get; set; }
        public UserFlag Flags { get; set; }
        public List<UserInfractionModel> Infractions { get; set; }
    }
}
