using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Logging;
using Silk.Core.Discord.Commands.General.Tickets;
using Silk.Core.Discord.EventHandlers.Notifications;
using Silk.Core.Discord.Services.Interfaces;

namespace Silk.Core.Discord.EventHandlers.MessageAdded
{
    public class MessageCreatedHandler
    {
        private readonly TicketService _ticketService;
        private readonly IPrefixCacheService _prefixCache;
        private readonly ILogger<MessageCreatedHandler> _logger;
        public MessageCreatedHandler(TicketService ticketService, IPrefixCacheService prefixCache, ILogger<MessageCreatedHandler> logger)
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
                    await member.SendMessageAsync(embed);
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

        public async Task Handle(MessageCreated notification, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}