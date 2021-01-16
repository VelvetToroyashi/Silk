namespace Silk.Core.Database.Models
{
    public sealed class BlackListedWord
    {
        public int Id { get; set; }
        public GuildConfigModel Guild { get; set; }
        public string Word { get; set; }
    }
}