using Newtonsoft.Json;
using SilkBot.Economy;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SilkBot.ServerConfigurations
{
    public class ServerConfigurationManager
    {
        public static ServerConfigurationManager Instance { get; } = new ServerConfigurationManager();
        public static ConcurrentDictionary<ulong, GuildInfo> LocalConfiguration { get; } = new ConcurrentDictionary<ulong, GuildInfo>();

        private ServerConfigurationManager()
        {
        }

        public void LoadServerConfigs()
        {
            foreach (var file in Directory.GetFiles(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SilkBot", "ServerConfigs")))
            {
                if (file.EndsWith(".gconfig")) continue;
                var fileContent = File.ReadAllText(file);
                var currentServerConfiguration = JsonConvert.DeserializeObject<GuildInfo>(fileContent);
                LocalConfiguration.TryAdd(currentServerConfiguration.Guild, currentServerConfiguration);
            }
            var globalConfigFilepath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SilkBot", "ServerConfigs", "GlobalConfig.gconfig");
            var globalConfigText = File.ReadAllText(globalConfigFilepath);
            Bot.GlobalConfig = JsonConvert.DeserializeObject<GlobalUserConfiguration>(globalConfigText);
            for (int i = 0; i < Bot.GlobalConfig.EconomicUsers.Count; i++)
                EconomicUsers.Instance.Users.TryAdd(Bot.GlobalConfig.EconomicUsers[i].UserId, Bot.GlobalConfig.EconomicUsers[i]);

            var prefixes = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SilkBot", "ServerConfigs", "prefixes.gconfig");
            Bot.GuildPrefixes = JsonConvert.DeserializeObject<Dictionary<ulong, string>>(File.ReadAllText(prefixes));
        }

        public async Task<GuildInfo> GenerateConfigurationFromIdAsync(ulong guildId)
        {
            var client = Bot.Instance.Client;
            var guild = await client.GetGuildAsync(guildId);
            var administrators = await ServerInfo.Instance.GetAdministratorsAsync(guild);
            var moderators = await ServerInfo.Instance.GetModeratorsAsync(guild);
            var bannedMembers = await ServerInfo.Instance.GetBansAsync(guild);
            var config = new GuildInfo
            {
                Administrators = administrators.ToList(),
                BannedMembers = bannedMembers.ToList(),
                Guild = guild.Id,
                Moderators = moderators.ToList()
            };
            LocalConfiguration.TryAdd(guildId, config);
            var json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SilkBot", "ServerConfigs", $"{guildId}.serverconfig"), json);
            return config;
        }
    }
}