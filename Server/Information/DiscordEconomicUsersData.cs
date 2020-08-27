using Newtonsoft.Json;
using SilkBot.Economy;
using System.Collections.Generic;

namespace SilkBot.ServerConfigurations
{
    public sealed class DiscordEconomicUsersData
    {
        [JsonProperty(PropertyName = "Economic Users")]
        public List<DiscordEconomicUser> EconomicUsers { get; set; } = new List<DiscordEconomicUser>();
    }
}
