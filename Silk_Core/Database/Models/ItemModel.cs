using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SilkBot.Database.Models
{
    public class ItemModel
    {
        [Key]
        public int Id { get; set; }
        public GlobalUserModel Owner { get; set; }
        [Column(TypeName = "jsonb")]
        public string State { get; set; }
    }
}