﻿using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System.Threading.Tasks;

namespace SilkBot.Server
{
    public sealed class GuildMemberCountChangeHandler
    {

        private readonly DiscordEmbedBuilder memberLeftEmbed = new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithTitle("Member Left!").WithFooter("Silk!", Bot.Instance.Client.CurrentUser.AvatarUrl);
        private readonly DiscordEmbedBuilder memberJoinedEmbed = new DiscordEmbedBuilder().WithColor(DiscordColor.Green).WithTitle("Member Joined!").WithFooter("Silk!", Bot.Instance.Client.CurrentUser.AvatarUrl);

        public async Task OnGuildMemberJoined(DiscordClient c, GuildMemberAddEventArgs e)
        {
            await Task.CompletedTask;
        }
        public async Task OnGuildMemberLeft(DiscordClient c, GuildMemberRemoveEventArgs e)
        {
            //var guild = dataContainerReference[e.Guild].GuildInfo;
            //if (guild.BannedMembers.Any(member => member.ID == e.Member.Id)) return;
            await Task.CompletedTask;
            //Do something useful here as well.

        }
    }
}
