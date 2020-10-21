namespace SilkBot.Models
{
    public sealed class BlackListedWord
    {
        public int Id { get; set; }
        public Guild Guild { get; set; }
        public string Word { get; set; }
    }
}