using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Silk.Core.Utilities.Bot
{
    public record BotConfig
    {
        // Took them out; they make no sense. //
        internal KeyValuePair<string, string> e6API { get; }


        internal BotConfig(IConfiguration c)
        {
            string? e6k = c.GetSection("API_Keys")["e621:key"];
            string? e6u = c.GetSection("API_Keys")["e621:user"];
            e6API = new(e6k, e6u);
        }
    }
}