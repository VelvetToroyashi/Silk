using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using SilkBot.Models;
using System;
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
            // Could use CommandTimer.Restart();
                        
            if (e.Author.IsBot)
            {
                CommandTimer.Stop();
                return;
            }
            
            var config = Instance.SilkDBContext.Guilds.FirstOrDefault(guild => guild.DiscordGuildId == e.Guild.Id);
            CommandTimer.Restart();
            //if (e.Channel.IsPrivate) await CheckForTicket(e);
            //Using .GetAwaiter has results in ~50x performance because of async overhead.
            CheckForInvite(e, config);
            Console.WriteLine($"Scanned for an invite in message in {CommandTimer.ElapsedTicks / 10L} µs.");

            var commands = Instance.Client.GetCommandsNext();
            var guildPrefix = config?.Prefix ?? SilkDefaultCommandPrefix;
            var prefixPos = e.Message.GetStringPrefixLength(guildPrefix);
            if (prefixPos < 1)
            {
                CommandTimer.Stop();
                return;
            }
            var messageContent = e.Message.Content.Substring(prefixPos);

            var command = commands.FindCommand(messageContent, out var args);
            var context = commands.CreateContext(e.Message, guildPrefix, command, args);
            if (command is null)
            {
                CommandTimer.Stop();
                return;
            }
            
            /* NOTE: 'ExecuteCommandAsync' method is wrapped in a try/catch internally so Exceptions will not be rethrown from commands */
            _ = Task.Run(async () => await commands.ExecuteCommandAsync(context));
            CommandTimer.Stop();
        }

        private void CheckForInvite(MessageCreateEventArgs e, Guild config)
        {
            if (config.WhiteListInvites)
            {
                var messageContent = e.Message.Content;
                if (messageContent.Contains("discord.gg") || 
                    messageContent.Contains("discord.com/invite"))
                {
                    var inviteLinkMatched = Regex.Match(messageContent, @"(discord\.gg\/.+)") 
                                 ?? Regex.Match(messageContent.ToLower(), @"(discord\.com\/invite\/.+)");
                    
                    if (!inviteLinkMatched.Success)
                    {
                        return;
                    }

                    var inviteLink = string.Join("", messageContent
                        .Skip(inviteLinkMatched.Index)
                        .TakeWhile(c => c != ' '))
                        .Replace("discord.com/invite", "discord.gg/");
                    
                    if (!config.WhiteListedLinks.Any(link => link.Link == inviteLink))
                    {
                        e.Message.DeleteAsync().GetAwaiter();
                    }
                }
            }
        }

        private async Task CheckForTicket(MessageCreateEventArgs e)
        {
            var ticket = Instance.SilkDBContext.Tickets.AsQueryable().OrderBy(_ => _.Opened).LastOrDefault(ticketModel => ticketModel.Opener == e.Message.Author.Id);

            // Can use null-propagation because (default(IEnumerable) or reference type is null)
            if (ticket?.Responders is null)
            {
                return;
            }

            if (!e.Channel.IsPrivate)
            {
                return;
            }

            if (ticket.IsOpen && !ticket.Responders.Any(responder => responder.ResponderId == e.Message.Author.Id))
            {
                foreach (var responder in ticket.Responders.Select(r => r.ResponderId))
                {
                    await Instance.Client.PrivateChannels.Values
                        .FirstOrDefault(c => c.Users.Any(u => u.Id == responder))
                        .SendMessageAsync("yesn't");
                }
            }
        }
    }
}
