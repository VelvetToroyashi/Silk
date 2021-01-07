using System;
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
        
        public Task Tickets(DiscordClient c, MessageCreateEventArgs e) => Task.Run(async () =>
        {
            if (!await _ticketService.HasTicket(e.Channel, e.Author.Id)) return;
            ulong ticketUser = TicketService.GetTicketUser(e.Channel);
            IEnumerable<KeyValuePair<ulong, DiscordMember?>> members = c.Guilds.Values.SelectMany(g => g.Members);
            DiscordMember? member = members.SingleOrDefault(m => m.Key == ticketUser).Value;
            if (member is null) return; // Member doesn't exist anymore // 
            DiscordEmbed embed = TicketEmbedHelper.GenerateOutboundEmbed(e.Message.Content, e.Author);
            await member.SendMessageAsync(embed: embed).ConfigureAwait(false);
        });

        public async Task Commands(DiscordClient c, MessageCreateEventArgs e) => // 'async' caused exceptions to get swallowed for some reason. //
            _ = Task.Run(async () =>
            {
                if (e.Author == c.CurrentUser || e.Author.IsBot || string.IsNullOrEmpty(e.Message.Content)) return Task.CompletedTask;
                CommandsNextExtension cnext = c.GetCommandsNext();

                string prefix = _prefixCache.RetrievePrefix(e.Guild?.Id) ?? string.Empty;

                int commandLength = e.MentionedUsers.Any(u => u.Id == c.CurrentUser.Id) ? e.Message.GetMentionPrefixLength(c.CurrentUser) : e.Message.GetStringPrefixLength(prefix);
                if (commandLength is -1) return Task.CompletedTask;

                string commandString = e.Message.Content.Substring(commandLength);

                Command? command = cnext.FindCommand(commandString, out string arguments);

                if (command is null) throw new CommandNotFoundException(commandString);

                CommandContext context = cnext.CreateContext(e.Message, prefix, command, arguments);

                _ = cnext.ExecuteCommandAsync(context);
                return Task.CompletedTask;
            });


    }
}