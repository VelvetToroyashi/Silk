namespace Silk.Core.Database.Models
{
    public class WhiteListedLink
    {
        public int Id { get; set; }
        public string Link { get; set; }

        public bool GuildLevelLink { get; set; }
        public GuildConfigModel Guild { get; set; }
    }
}