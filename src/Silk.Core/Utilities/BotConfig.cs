using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Silk.Core.Utilities
{
    public record BotConfig
    {
        // Took them out; they make no sense. //
        internal KeyValuePair<string, string> e6API { get; init; }


        public BotConfig(IConfiguration c)
        {
            var e6k = c.GetSection("API_Keys")["e621:key"];
            var e6u = c.GetSection("API_Keys")["e621:user"];
            e6API = new KeyValuePair<string, string>(e6k, e6u);

        }
    }
}