using Newtonsoft.Json;
using SilkBot.Economy;
using System.Collections.Generic;

namespace SilkBot.ServerConfigurations
{
    public sealed class GlobalUserConfiguration
    {
        [JsonProperty]
        public List<DiscordEconomicUser> EconomicUsers { get; set; } = new List<DiscordEconomicUser>();
    }
}