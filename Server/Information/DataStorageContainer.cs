using DSharpPlus;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using SilkBot.Economy;
using SilkBot.ServerConfigurations.UserInfo;
using SilkBot.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SilkBot.ServerConfigurations
{
    public class DataStorageContainer
    {
        [JsonProperty(PropertyName = "Guild Information List")]
        private readonly List<GuildInfo> guildInfos;

        [JsonProperty(PropertyName = "Economic Users")]
        private DiscordEconomicUsersData economicusers;

        public (GuildInfo GuildInfo, List<DiscordEconomicUser> EconomicUsers) this[DiscordGuild guild] => (guildInfos.Single(g => g.GuildId == guild.Id), economicusers.EconomicUsers);

        public DataStorageContainer()
        {
            guildInfos = new List<GuildInfo>();
            economicusers = new DiscordEconomicUsersData();
        }

        public void PopulateDataOnApplicationLoad()
        {
            var sw = new Stopwatch();
            sw.Start();
            var economicUserDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SilkBot", "Configs", "GlobalConfig.gconfig");
            var economicUserData = File.ReadAllText(economicUserDataPath);
            economicusers = JsonConvert.DeserializeObject<DiscordEconomicUsersData>(economicUserData);

            var economicUserDataLoadTime = sw.ElapsedMilliseconds;
            Console.WriteLine($"[{DateTime.Now:HH:MM}] Loaded economic user data in {economicUserDataLoadTime} ms!");

            var configDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SilkBot", "Configs");
            foreach (var file in Directory.GetFiles(configDataPath))
            {
                if (file.EndsWith(".gconfig")) continue;
                var serverInformation = File.ReadAllText(file);
                var serverInformationObject = JsonConvert.DeserializeObject<GuildInfo>(serverInformation);
                guildInfos.Add(serverInformationObject);
            }
            Console.WriteLine($"[{DateTime.Now:HH:MM}] Intialized all server objects in " +
                $"{sw.ElapsedMilliseconds - economicUserDataLoadTime} ms. \n" +
                $"[{DateTime.Now:HH:MM}] Total time: {sw.ElapsedMilliseconds} ms");
            sw.Stop();
        }

        public void SaveServerData()
        {
            var economicUserDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SilkBot", "Configs", "GlobalConfig.gconfig");
            var economicUserData = JsonConvert.SerializeObject(economicusers);
            File.WriteAllText(economicUserDataPath, economicUserData);
            Console.WriteLine($"[{DateTime.Now:HH:MM}]Saved global data.");

            var instancedServerDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SilkBot", "Configs");
            foreach (var serverInformationFile in guildInfos)
            {
                var serverInformationObject = JsonConvert.SerializeObject(serverInformationFile);
                File.WriteAllText(Path.Combine(instancedServerDataPath, serverInformationFile.GuildId + ".serverconfig"), serverInformationObject);
            }
            Console.WriteLine($"Saved guild information for {guildInfos.Count} guilds.");
        }

        public async Task GenerateGuildInfoObjectAsync(DiscordGuild guild)
        {
            var allDiscordMembers = await guild.GetAllMembersAsync();

            var rawAdministratorDiscordMembers = allDiscordMembers.Where(member => member.HasPermission(Permissions.Administrator));
            var rawModeratorDiscordmembers = allDiscordMembers.Where(member => member.HasPermission(Permissions.KickMembers)).Except(rawAdministratorDiscordMembers);

            var moderatorAsModeratorObject = rawModeratorDiscordmembers.Select(mod => new Moderator(mod.Id));
            var administratorAsAdministratorObject = rawAdministratorDiscordMembers.Select(admin => new Administrator(admin.Id));

            var BannedMembers = (await guild.GetBansAsync()).Select(ban => new BannedMember(ban.User.Id, ban.Reason));

            var guildInfo = new GuildInfo
            {
                Administrators = administratorAsAdministratorObject.ToList(),
                Moderators = moderatorAsModeratorObject.ToList(),
                BannedMembers = BannedMembers.ToList(),
                GuildId = guild.Id,
            };

            guildInfos.Add(guildInfo);
        }

        public async Task FetchGuildInfo(IEnumerable<DiscordGuild> guilds)
        {
            foreach (var guild in guilds)
            {
                if (guildInfos.Select(g => g.GuildId).Any(g => g == guild.Id)) continue;
                await GenerateGuildInfoObjectAsync(guild);
            }
        }
    }
}