namespace SilkBot.Models
{
    public sealed class BlackListedWord
    {
        public int Id { get; set; }
        public GuildModel Guild { get; set; }
        public string Word { get; set; }
    }
}