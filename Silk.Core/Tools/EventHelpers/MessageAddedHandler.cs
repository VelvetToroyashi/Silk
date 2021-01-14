using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Serilog;
using Silk.Core.Commands.General.Tickets;
using Silk.Core.Services;

namespace Silk.Core.Tools.EventHelpers
{
    public class MessageAddedHandler
    {
        private readonly TicketService _ticketService;
        private readonly PrefixCacheService _prefixCache;

        public MessageAddedHandler(TicketService ticketService, PrefixCacheService prefixCache)
        {
            _ticketService = ticketService;
            _prefixCache = prefixCache;
        }

        public Task Tickets(DiscordClient c, MessageCreateEventArgs e) =>
            _ = Task.Run(async () =>
            {
                if (!await _ticketService.HasTicket(e.Channel, e.Author.Id)) return;

                ulong ticketUserId = TicketService.GetTicketUser(e.Channel);
                IEnumerable<KeyValuePair<ulong, DiscordMember?>> members = c.Guilds.Values.SelectMany(g => g.Members);
                DiscordMember? member = members.SingleOrDefault(m => m.Key == ticketUserId).Value;

                if (member is null) return; // Member doesn't exist anymore // 

                DiscordEmbed embed = TicketEmbedHelper.GenerateOutboundEmbed(e.Message.Content, e.Author);

                await member.SendMessageAsync(embed: embed).ConfigureAwait(false);
            });

        public async Task Commands(DiscordClient c, MessageCreateEventArgs e) =>
            _ = Task.Run(async () =>
            {
                if (e.Author.IsBot || string.IsNullOrEmpty(e.Message.Content)) return;
                CommandsNextExtension cnext = c.GetCommandsNext();

                string prefix = _prefixCache.RetrievePrefix(e.Guild?.Id);
                
                int prefixLength = 
                    e.Channel.IsPrivate ? 0 : // No prefix in DMs, else try to get the string prefix length. //
                        e.MentionedUsers.Any(u => u.Id == c.CurrentUser.Id) ?
                        e.Message.GetMentionPrefixLength(c.CurrentUser) : 
                        e.Message.GetStringPrefixLength(prefix);
                    
                if (prefixLength is -1) return;

                string commandString = e.Message.Content.Substring(prefixLength);

                Command? command = cnext.FindCommand(commandString, out string arguments);

                if (command is null)
                {
                    Log.Logger.Warning($"Command not found: {commandString}");
                    return;
                }
                CommandContext context = cnext.CreateContext(e.Message, prefix, command, arguments);

                await cnext.ExecuteCommandAsync(context);
            });
    }
}