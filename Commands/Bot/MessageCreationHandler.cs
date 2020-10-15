using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using Silk__Extensions;
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
        private readonly IDbContextFactory<SilkDbContext> dbContextFactory;
        public MessageCreationHandler()
        {
            Instance.Client.MessageCreated += OnMessageCreate;
            dbContextFactory = Instance.Services.Get<IDbContextFactory<SilkDbContext>>();
        }

        private async Task OnMessageCreate(DiscordClient c, MessageCreateEventArgs e)
        {
            //Bots shouldn't be running commands.    
            if (e.Author.IsBot)
            {
                CommandTimer.Stop();
                return;
            }
            e.Handled = true;
            //Silk specific, but feel free to use the same code, modified to fit your DB or other prefix-storing method.
            var config = dbContextFactory.CreateDbContext().Guilds.FirstOrDefault(guild => guild.DiscordGuildId == e.Guild.Id);
            CommandTimer.Restart();
            //if (e.Channel.IsPrivate) await CheckForTicket(e);
            //Using .GetAwaiter has results in ~50x performance because of async overhead.
            CheckForInvite(e, config);
            Console.WriteLine($"Scanned for an invite in message in {CommandTimer.ElapsedTicks / 10} µs.");
            //End of Silk specific code//


            var commands = c.GetCommandsNext();
            var guildPrefix = config?.Prefix ?? SilkDefaultCommandPrefix;
            var prefixPos = e.Message.GetStringPrefixLength(guildPrefix);
            if (prefixPos < 1)
            {
                //prefix wasn't found in the message
                CommandTimer.Stop();
                return;
            }
            var messageContent = e.Message.Content.Substring(prefixPos);

            var command = commands.FindCommand(messageContent, out var args);
            var context = commands.CreateContext(e.Message, guildPrefix, command, args);
            if (command is null)
            {
                //Invalid command; feel free to throw an exception / Send an error message via e.Channel.SendMessage()
                CommandTimer.Stop();
                return;
            }
            
            /* NOTE: 'ExecuteCommandAsync' method is wrapped in a try/catch internally so Exceptions will not be rethrown from commands */
            _ = Task.Run(async () => await commands.ExecuteCommandAsync(context));
            //Very important that you do NOT *EVER* await commands, especially in the case of Interactivity; it will deadlock your bot.//
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
