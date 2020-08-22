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
        public void Add(DiscordMember member)
        {
            var Id = member.Id;
            var user = new DiscordEconomicUser(Id, member.DisplayName);
            if (Users.ContainsKey(Id)) return;
            Users.TryAdd(Id, user);
            Bot.EconomicUsers.EconomicUsers.Add(user); 
        }
        //I don't use this as much as I could and should. Sad.//
        public bool UserExists(ulong Id) => Users.ContainsKey(Id);

        private EconomicUsers() { }
    }
}
