using Microsoft.Extensions.Configuration;

namespace SilkBot.Utilities
{
    public record BotConfig
    {
        // Took them out; they make no sense. //
        internal string e621APIKey      { get; init; }
        internal string e621Username    { get; init; }
        

        public BotConfig(IConfiguration c)
        {
            e621APIKey = c.GetSection("API_Keys")["e621_Key"];
            e621Username = c.GetSection("API_Keys")["e621_User"];
        }
    }
}