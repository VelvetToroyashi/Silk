using Microsoft.Extensions.Configuration;

namespace SilkBot.Utilities
{
    public record BotConfig
    {
        // Not quite permenant. //
        // I'll probably end up removing this, actually. It doesn't serve much purpose ~Velvet //
        internal string Token           { get; init; }
        internal string Connection      { get; init; }
        
        internal string e621APIKey      { get; init; }
        internal string e621Username    { get; init; }
        

        public BotConfig(IConfiguration c)
        {
            Connection = c.GetConnectionString("DbConnection");
            Token = c.GetConnectionString("Token");
            e621APIKey = c.GetSection("API_Keys")["e621_Key"];
            e621Username = c.GetSection("API_Keys")["e621_User"];
        }
    }
}