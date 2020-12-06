using System.ComponentModel.DataAnnotations;

namespace SilkBot.Models
{
    public sealed class BlackListedWord
    {
        public int Id { get; set; }
        public GuildModel Guild { get; set; } = new();
        public string Word { get; set; } = string.Empty;


    }
}