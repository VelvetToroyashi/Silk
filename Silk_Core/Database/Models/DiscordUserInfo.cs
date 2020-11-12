using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SilkBot.Models
{
    public class UserModel
    {
        [Key]
        public ulong Id { get; set; }
        public GuildModel Guild { get; set; }
        public UserFlag Flags { get; set; }
        public List<UserInfractionModel> Infractions { get; set; }
    }
}
