using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using SilkBot.ServerConfigurations;
using System.Collections.Concurrent;

namespace SilkBot.Economy
{
    public sealed class EconomicUsers
    {
        public ConcurrentDictionary<ulong, DiscordEconomicUser> Users { get; } = new ConcurrentDictionary<ulong, DiscordEconomicUser>();
        public static EconomicUsers Instance { get; } = new EconomicUsers();

        //I think I commented this out because the method below is better and improved? Don't quote me.//

        //public async void Add(ulong Id, DiscordEconomicUser user, DiscordGuild guild)
        //{
        //    if (Users.ContainsKey(Id))
        //        return;
        //    Users.TryAdd(Id, user);
        //    ServerConfigurationManager.Configs.TryGetValue(guild.Id, out var config);
        //    if (config is null)
        //    {
        //        ServerConfigurationManager.Configs.TryAdd(guild.Id, await ServerConfigurationManager.Instance.GenerateConfigurationFromIdAsync(guild.Id));
        //        ServerConfigurationManager.Configs[guild.Id].EconomicUsers.Add(user);
        //        //config.EconomicUsers.Add(user);
        //        //File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SilkBot", "ServerConfigs") + $"{guild.Id}.serverconfig", JsonConvert.SerializeObject(config));
        //    }
        //    else
        //    {
        //        ServerConfigurationManager.Configs[guild.Id].EconomicUsers.Add(user);
        //    }
        //}

        public async void Add(CommandContext ctx, DiscordMember member)
        {
            var Id = member.Id;
            var guild = ctx.Guild;
            var user = new DiscordEconomicUser(Id, member.DisplayName);
            if (Users.ContainsKey(Id))
                return;
            Users.TryAdd(Id, user);
            ServerConfigurationManager.Configs.TryGetValue(guild.Id, out var config);
            //Is the configuration we received null? Make a new one if it is, else add to the one that already exists.//

            if (config is null)
            {
                ServerConfigurationManager.Configs.TryAdd(guild.Id, await ServerConfigurationManager.Instance.GenerateConfigurationFromIdAsync(guild.Id));
                ServerConfigurationManager.Configs[guild.Id].EconomicUsers.Add(user);

                //This is handled.//
                //config.EconomicUsers.Add(user);
                //File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SilkBot", "ServerConfigs") + $"{guild.Id}.serverconfig", JsonConvert.SerializeObject(config));
                
            }
            else
            {
                ServerConfigurationManager.Configs[guild.Id].EconomicUsers.Add(user);
            }
        }
        //I don't use this as much as I could and should. Sad.//
        public bool UserExists(ulong Id) => Users.ContainsKey(Id);

        private EconomicUsers() { }
    }
}
