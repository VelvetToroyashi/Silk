using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using SilkBot.ServerConfigurations.UserInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilkBot.ServerConfigurations
{
    public class ServerInfo
    {
        public static ServerInfo Instance { get; } = new ServerInfo();

        private ServerInfo() { }



        public async Task<Moderator[]> GetModeratorsAsync(DiscordGuild guild)
        {
            var members = await guild.GetAllMembersAsync();
            var ModeratorObjectList = members.Where(member => member.Roles.Any(role => role.Permissions.HasPermission(Permissions.KickMembers)));
            var ModeratorIDList = new List<Moderator>();
            foreach (var admin in ModeratorObjectList)
                ModeratorIDList.Add(new Moderator() { ID = admin.Id });
            return ModeratorIDList.ToArray();
        }
        public async Task<BannedMember[]> GetBansAsync(DiscordGuild guild)
        {
            var bannedUsersArray = await guild.GetBansAsync();
            var bannedUsersList = new List<BannedMember>();
            foreach (var ban in bannedUsersArray.ToArray())
                bannedUsersList.Add(new BannedMember() { ID = ban.User.Id, Reason = ban.Reason });
            return bannedUsersList.ToArray();


        }
        public async Task<Administrator[]> GetAdministratorsAsync(DiscordGuild guild)
        {
            var members = await guild.GetAllMembersAsync();
            var AdministratorObjectList = members.Where(member => member.Roles.Any(role => role.Permissions.HasPermission(Permissions.KickMembers)));
            var AdministratorIDList = new List<Administrator>();
            foreach (var admin in AdministratorObjectList)
                AdministratorIDList.Add(new Administrator() { ID = admin.Id });
            return AdministratorIDList.ToArray();
        }

        public async Task<DiscordChannel> ReturnChannelFromID(CommandContext commandContext, ulong Id)
        {
            if (Id == 0)
                return null;

            return await commandContext.Client.GetChannelAsync(Id);

        }


    }
}
