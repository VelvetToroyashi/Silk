namespace Silk.Core.Database.Models
{
    public class RoleMenuReactionModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ulong EmojiId { get; set; }
        public ulong? RequiredRole { get; set; }
    }
}