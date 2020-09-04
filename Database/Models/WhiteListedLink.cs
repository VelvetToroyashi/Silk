namespace SilkBot.Models
{
    public class WhiteListedLink
    {
        public int Id { get; set; }
        public Guild Guild { get; set; }
        public string Link { get; set; }
    }
}