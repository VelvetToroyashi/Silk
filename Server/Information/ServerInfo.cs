using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using SilkBot.ServerConfigurations.UserInfo;
using SilkBot.Utilities;
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



        public async Task<IEnumerable<Moderator>> GetModeratorsAsync(DiscordGuild guild) =>
            (await guild.GetAllMembersAsync())
            .Where(member => member.HasPermission(Permissions.KickMembers) && !member.IsBot)
                .Select(mod => new Moderator(mod.Id));
        //public async Task<IEnumerable<BannedMember>> GetBansAsync(DiscordGuild guild) =>
        //    (await guild.GetBansAsync()).Select(ban => new BannedMember(ban.User.Id, ban.Reason));

        public async Task<IEnumerable<Administrator>> GetAdministratorsAsync(DiscordGuild guild) =>
            (await guild.GetAllMembersAsync())
            .Where(member => member.HasPermission(Permissions.Administrator) && !member.IsBot)
                .Select(admin => new Administrator(admin.Id));

        

        public async Task<DiscordChannel> ReturnChannelFromID(CommandContext commandContext, ulong Id)
        {
            return Id == 0 ? null : await commandContext.Client.GetChannelAsync(Id);
        }


    }
}
