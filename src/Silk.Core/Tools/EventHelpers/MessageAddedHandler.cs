using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Logging;
using Silk.Core.Commands.General.Tickets;
using Silk.Core.Services;

namespace Silk.Core.Tools.EventHelpers
{
    public class MessageAddedHandler
    {
        private readonly TicketService _ticketService;
        private readonly PrefixCacheService _prefixCache;
        private readonly ILogger<MessageAddedHandler> _logger;
        public MessageAddedHandler(TicketService ticketService, PrefixCacheService prefixCache, ILogger<MessageAddedHandler> logger)
        {
            _ticketService = ticketService;
            _prefixCache = prefixCache;
            _logger = logger;
        }

        public Task Tickets(DiscordClient c, MessageCreateEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                if (!await _ticketService.HasTicket(e.Channel, e.Author.Id)) return;

                ulong ticketUserId = TicketService.GetTicketUser(e.Channel);
                IEnumerable<KeyValuePair<ulong, DiscordMember?>> members = c.Guilds.Values.SelectMany(g => g.Members);
                DiscordMember? member = members.SingleOrDefault(m => m.Key == ticketUserId).Value;

                if (member is null) return; // Member doesn't exist anymore // 

                DiscordEmbed embed = TicketEmbedHelper.GenerateOutboundEmbed(e.Message.Content, e.Author);
                try
                {
                    await member.SendMessageAsync(embed).ConfigureAwait(false);
                }
                catch (UnauthorizedException)
                {
                    var builder = new DiscordMessageBuilder();
                    builder.WithReply(e.Message.Id, true);
                    builder.WithContent("I couldn't message that user! They've either closed their DMs or left all mutual servers!");
                    
                    await e.Channel.SendMessageAsync(builder);
                }
            });
            return Task.CompletedTask;
        }
        
        public Task Commands(DiscordClient c, MessageCreateEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                if (e.Author.IsBot || string.IsNullOrEmpty(e.Message.Content)) return;
                CommandsNextExtension cnext = c.GetCommandsNext();

                string prefix = _prefixCache.RetrievePrefix(e?.Guild?.Id);
                
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
                    _logger.LogWarning($"Command not found: {e.Message.Content}");
                    return;
                }
                CommandContext context = cnext.CreateContext(e.Message, prefix, command, arguments);

                await cnext.ExecuteCommandAsync(context).ConfigureAwait(false);
            });
            return Task.CompletedTask;
        }
            
    }
}