using System.ComponentModel.DataAnnotations.Schema;

namespace Silk.Core.Database.Models
{
    public class ItemModel
    {
        public int Id { get; set; }
        public GlobalUserModel Owner { get; set; }
        [Column(TypeName = "jsonb")]
        public string State { get; set; }
    }
}