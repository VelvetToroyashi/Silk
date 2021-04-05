using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using MediatR;
using Silk.Core.Discord.Commands.General.Tickets;
using Silk.Core.Discord.EventHandlers.Notifications;

namespace Silk.Core.Discord.EventHandlers.MessageAdded
{
    public class TicketHandler : INotificationHandler<MessageCreated>
    {
        private readonly TicketService _ticketService;
        public TicketHandler(TicketService ticketService) => _ticketService = ticketService;

        // Send help. //
        public async Task Tickets(DiscordClient c, MessageCreateEventArgs e)
        {
            if (!await _ticketService.HasTicket(e.Channel, e.Author.Id)) return;

            ulong ticketUserId = TicketService.GetTicketUser(e.Channel);

            IEnumerable<KeyValuePair<ulong, DiscordMember?>> members = c.Guilds.Values.SelectMany(g => g.Members);
            DiscordMember? member = members.SingleOrDefault(m => m.Key == ticketUserId).Value;

            if (member is null) return; // Member doesn't exist anymore, or wasn't in cache, which they should be :sus: // 

            DiscordEmbed embed = TicketEmbedHelper.GenerateOutboundEmbed(e.Message.Content, e.Author); // Should've renamed this tbqh                  //
            try { await member.SendMessageAsync(embed); } // Outbound is for what the ticket opener sees. //
            catch (UnauthorizedException)
            {
                var builder = new DiscordMessageBuilder();
                builder.WithReply(e.Message.Id, true);
                builder.WithContent("I couldn't message that user! They've either closed their DMs.");

                await e.Channel.SendMessageAsync(builder);
            }
        }

        public async Task Handle(MessageCreated notification, CancellationToken cancellationToken)
        {
            await Tickets(notification.Client, notification.EventArgs);
        }
    }
}