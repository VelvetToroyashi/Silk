using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using Silk__Extensions;
using SilkBot.Commands.General;
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
        private readonly TicketService _ticketService = new TicketService(Instance.Client);
        public MessageCreationHandler()
        {
            dbContextFactory = Instance.Services.Get<IDbContextFactory<SilkDbContext>>();
        }

        public async Task OnMessageCreate(DiscordClient c, MessageCreateEventArgs e)
        {
            //Bots shouldn't be running commands.    
            if (e.Author.IsBot)
            {
                CommandTimer.Stop();
                return;
            }
            e.Handled = true;
            //Silk specific, but feel free to use the same code, modified to fit your DB or other prefix-storing method.
            
            CommandTimer.Restart();
            if (_ticketService.CheckForTicket(e.Channel, e.Message.Author.Id)) { await _ticketService.RespondToBlindTicket(c, e.Author.Id, e.Message.Content); }

            var config = dbContextFactory.CreateDbContext().Guilds.FirstOrDefault(guild => guild.DiscordGuildId == (e.Guild == null ? 0 : e.Guild.Id));
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
            if (config is null) return; //Channel is privatate, so no guild exists.
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


    }
}
