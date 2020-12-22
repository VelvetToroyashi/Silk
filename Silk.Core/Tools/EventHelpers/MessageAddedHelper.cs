#region

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Silk.Core.Commands.General.Tickets;
using Silk.Core.Services;

#endregion

namespace Silk.Core.Tools.EventHelpers
{
    public class MessageAddedHelper
    {
        private readonly TicketHandlerService _ticketService;
        private readonly PrefixCacheService _prefixCache;

        public MessageAddedHelper(TicketHandlerService ticketService, PrefixCacheService prefixCache)
        {
            _ticketService = ticketService;
            _prefixCache = prefixCache;
        }
        
        public async Task Tickets(DiscordClient c, MessageCreateEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                if (!await _ticketService.HasTicket(e.Channel, e.Author.Id)) return;
                ulong ticketUser = TicketHandlerService.GetTicketUser(e.Channel);
                IEnumerable<KeyValuePair<ulong, DiscordMember?>> members = c.Guilds.Values.SelectMany(g => g.Members);
                DiscordMember? member = members.SingleOrDefault(m => m.Key == ticketUser).Value;
                if (member is null) return; // Member doesn't exist anymore // 
                DiscordEmbed embed = TicketEmbedHelper.GenerateOutboundEmbed(e.Message.Content, e.Author);
                await member.SendMessageAsync(embed: embed).ConfigureAwait(false);
            });
        }
        
        public async Task Commands(DiscordClient c, MessageCreateEventArgs e)
        {
            if (e.Author == c.CurrentUser) return;
            _ = Task.Run(async () =>
            {
                string prefix = _prefixCache.RetrievePrefix(e.Guild?.Id) ?? string.Empty;
                CommandsNextExtension cnext = c.GetCommandsNext();
                int commandLength =
                    e.MentionedUsers.Any(u => u.Id == c.CurrentUser.Id)
                        ? e.Message.GetMentionPrefixLength(c.CurrentUser)
                        : e.Message.GetStringPrefixLength(prefix);
                string? commandString = e.Message.Content.Substring(commandLength);
                //string? split = commandString?.Split()?[0];
                // if (e.Guild is not null &&
                //     (disabledCommandsCache.GetValueOrDefault(e.Guild.Id)?.Contains(split) ?? false)) return;
                
                Command? command = cnext.FindCommand(commandString, out string arguments);
                
                 if (command is null) 
                     throw new CommandNotFoundException(commandString);
                
                CommandContext context = cnext.CreateContext(e.Message, prefix, command, arguments);
                
                await cnext.ExecuteCommandAsync(context);
            });
        }
        
        
    }
}