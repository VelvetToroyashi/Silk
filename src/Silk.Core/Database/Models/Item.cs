using System.ComponentModel.DataAnnotations.Schema;

namespace Silk.Core.Database.Models
{
    public class Item
    {
        public int Id { get; set; }
        public GlobalUser Owner { get; set; }
        [Column(TypeName = "jsonb")]
        public string State { get; set; }
    }
}