using System.Collections.Generic;

namespace Silk.Core.Database.Models
{
    public class UserModel
    {
        public ulong Id { get; set; }
        public long DatabaseId { get; set; }
        public GuildModel Guild { get; set; }
        public UserFlag Flags { get; set; }
        public List<UserInfractionModel> Infractions { get; set; } = new();
    }
}