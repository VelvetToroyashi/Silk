using DSharpPlus.Entities;
using Newtonsoft.Json;
using SilkBot.Economy;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SilkBot.ServerConfigurations
{
    public class ServerConfigurationManager
    {
        private static ConcurrentDictionary<ulong, ServerConfig> configurations = new ConcurrentDictionary<ulong, ServerConfig>();
        public static ServerConfigurationManager Instance { get; } = new ServerConfigurationManager();
        public static ConcurrentDictionary<ulong, ServerConfig> Configs { get => configurations; }
        private ServerConfigurationManager() { }

        public void LoadServerConfigs()
        {   
            foreach(var file in Directory.GetFiles(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SilkBot", "ServerConfigs")))
            {
                var fileContent = File.ReadAllText(file);
                var currentServerConfiguration = JsonConvert.DeserializeObject<ServerConfig>(fileContent);
                configurations.TryAdd(currentServerConfiguration.Guild, currentServerConfiguration);
                for(int i = 0; i < currentServerConfiguration.EconomicUsers.Count; i++)
                    EconomicUsers.Instance.Users.TryAdd(currentServerConfiguration.EconomicUsers[i].UserId, currentServerConfiguration.EconomicUsers[i]);
            }

        }
        
        public async Task<ServerConfig> GenerateConfigurationFromIdAsync(ulong guildId)
        {
            var guild = await Bot.Instance.Client.GetGuildAsync(guildId);
            var administrators = await ServerInfo.Instance.GetAdministratorsAsync(guild);
            var moderators = await ServerInfo.Instance.GetModeratorsAsync(guild);
            var bannedMembers = await ServerInfo.Instance.GetBansAsync(guild);
            var economicUser = EconomicUsers.Instance.Users.Values.ToList();
            var config = new ServerConfig 
            { 
                Administrators = administrators, 
                BannedMembers = bannedMembers, 
                EconomicUsers = economicUser, 
                Guild = guild.Id,
                Moderators = moderators 
            };
            configurations.TryAdd(guildId, config);
            var json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SilkBot", "ServerConfigs", $"{guildId}.serverconfig"), json);
            return config;

        }


    }
}
