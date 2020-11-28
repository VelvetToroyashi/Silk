using System.ComponentModel.DataAnnotations.Schema;

namespace SilkBot.Database.Models
{
    public class ItemModel
    {
        public int Id { get; set; }
        
        public GlobalUserModel Owner { get; set; }

        [Column(TypeName = "jsonb")]
        public string InstanceState { get; set; }
    }
}