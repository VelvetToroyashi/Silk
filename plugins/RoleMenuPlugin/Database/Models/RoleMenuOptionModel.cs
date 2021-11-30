namespace RoleMenuPlugin.Database
{
    public sealed class RoleMenuOptionModel
    {
        public int Id { get; set; }

        public ulong RoleMenuId { get; set; }

        public ulong GuildId { get; set; }

        public ulong RoleId { get; set; }

        public string RoleName { get; set; }

        public ulong MessageId { get; set; }

        public string ComponentId { get; set; }

        public string? EmojiName { get; set; }

        public string? Description { get; set; }
    }

}