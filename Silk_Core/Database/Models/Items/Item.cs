using System.ComponentModel.DataAnnotations;

namespace SilkBot.Database.Models.Items
{
    public abstract class Item
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

    }
}
