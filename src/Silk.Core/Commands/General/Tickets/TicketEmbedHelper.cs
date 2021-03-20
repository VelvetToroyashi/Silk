using DSharpPlus.Entities;
using Silk.Core.Data.Models;
using Silk.Extensions.DSharpPlus;

namespace Silk.Core.Commands.General.Tickets
{
    public static class TicketEmbedHelper
    {
        public static DiscordEmbed GenerateOutboundEmbed(string message, DiscordUser responder) =>
            new DiscordEmbedBuilder()
                .WithTitle("Ticket response:")
                .WithAuthor(responder.Username, responder.GetUrl(), responder.AvatarUrl)
                .WithDescription(message)
                .WithFooter("Ticket history is saved for security purposes")
                .WithColor(DiscordColor.Goldenrod)
                .Build();

        public static DiscordEmbed GenerateInboundEmbed(string message, DiscordUser ticketOpener, Ticket ticket) =>
            new DiscordEmbedBuilder()
                .WithAuthor(ticketOpener.Username, null, ticketOpener.AvatarUrl)
                .WithColor(DiscordColor.DarkBlue)
                .WithDescription(message)
                .WithFooter($"Silk! | Ticket Id: {ticket.Id}");

        public static DiscordEmbed GenerateTicketClosedEmbed() =>
            new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Goldenrod)
                .WithTitle("Your ticket has been closed.")
                .WithDescription("Your ticket has been manually closed. If you have any futher issues, feel free to open a new ticket via `ticket create [message]`")
                .Build();
    }
}