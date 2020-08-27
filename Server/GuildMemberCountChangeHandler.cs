using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using SilkBot.ServerConfigurations;
using System.Linq;
using System.Threading.Tasks;

namespace SilkBot.Server
{
    public sealed class GuildMemberCountChangeHandler
    {
        private readonly DataStorageContainer dataContainerReference;
        private readonly DiscordEmbedBuilder memberLeftEmbed = new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithTitle("Member Left!").WithFooter("Silk!", Bot.Instance.Client.CurrentUser.AvatarUrl);
        private readonly DiscordEmbedBuilder memberJoinedEmbed = new DiscordEmbedBuilder().WithColor(DiscordColor.Green).WithTitle("Member Joined!").WithFooter("Silk!", Bot.Instance.Client.CurrentUser.AvatarUrl);
        public GuildMemberCountChangeHandler(ref DataStorageContainer _data, DiscordClient client)
        {
            dataContainerReference = _data;
            client.GuildMemberAdded += OnGuildMemberJoined;
            client.GuildMemberRemoved += OnGuildMemberLeft;

        }

        private async Task OnGuildMemberJoined(GuildMemberAddEventArgs e)
        {
            await Task.CompletedTask;
        }
        private async Task OnGuildMemberLeft(GuildMemberRemoveEventArgs e)
        {
            var guild = dataContainerReference[e.Guild].GuildInfo;
            if (guild.BannedMembers.Any(member => member.ID == e.Member.Id)) return;
            await Task.CompletedTask;
            //Do something useful here as well.

        }
    }
}
