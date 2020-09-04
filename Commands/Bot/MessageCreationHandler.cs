using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using SilkBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static SilkBot.Bot;

namespace SilkBot.Commands.Bot
{
    public sealed class MessageCreationHandler
    {
        public MessageCreationHandler() => Instance.Client.MessageCreated += OnMessageCreate;

        private async Task OnMessageCreate(MessageCreateEventArgs e)
        {
            var config = Instance.SilkDBContext.Guilds.AsEnumerable().FirstOrDefault(guild => guild.DiscordGuildId == e.Guild?.Id);
            

            if (e.Author.IsBot) return;
            //if (e.Channel.IsPrivate) await CheckForTicket(e);
            await CheckForInvite(e, config);
            var prefix = config?.Prefix ?? "!";
            var prefixPos = e.Message.GetStringPrefixLength(prefix);
            if (prefixPos < 0) return;
            var pfx = e.Message.Content.Substring(0, prefixPos);
            var cnt = e.Message.Content.Substring(prefixPos);

            var cmd = Instance.Client.GetCommandsNext().FindCommand(cnt, out var args);
            var ctx = Instance.Client.GetCommandsNext().CreateContext(e.Message, pfx, cmd, args);
            if (cmd is null) return;

            await Task.Run(async () => await Instance.Client.GetCommandsNext().ExecuteCommandAsync(ctx));
        }

        private async Task CheckForInvite(MessageCreateEventArgs e, Guild config)
        {
            if (config.WhiteListInvites)
            {
                if (e.Message.Content.Contains("discord.gg") || e.Message.Content.Contains("discord.com/invite"))
                {
                    var inviteIndex = (Regex.Match(e.Message.Content, @"(discord\.gg/)") ?? Regex.Match(e.Message.Content.ToLower(), @"(discord\.com\/invite\/)")).Index;
                    var inviteLink = string.Join("", e.Message.Content.Skip(inviteIndex).TakeWhile(c => c != ' ')).Replace("discord.com/invite", "discord.gg/");
                    if (!config.WhiteListedLinks.Any(link => link.Link == inviteLink)) await e.Message.DeleteAsync();
                }
            }
        }
        private async Task CheckForTicket(MessageCreateEventArgs e)
        {
            var ticket = Instance.SilkDBContext.Tickets.AsQueryable().OrderBy(_ => _.Opened).LastOrDefault(ticket => ticket.Opener == e.Message.Author.Id);
            if (ticket is null) return;
            if (ticket.Responders == default) return;
            if (!e.Channel.IsPrivate) return;
            if (ticket.IsOpen && !ticket.Responders.Any(responder => responder.ResponderId == e.Message.Author.Id))
            {
                foreach (var responder in ticket.Responders.Select(r => r.ResponderId))
                {
                    await Instance.Client.PrivateChannels.Values.FirstOrDefault(c => c.Users.Any(u => u.Id == responder)).SendMessageAsync("yesn't");
                }
            }
        }
    }
}
