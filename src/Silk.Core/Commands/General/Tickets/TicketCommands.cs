using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Core.Utilities;
using Silk.Core.Utilities.HelpFormatter;
using Silk.Data.Models;

namespace Silk.Core.Commands.General.Tickets
{
    /// <summary>
    /// Class responsible for the creation of tickets.
    /// </summary>
    [Experimental]
    [Group("ticket")]
    [Category(Categories.Bot)]
    [Description("CommandInvocations related to tickets; opening tickets can only be performed in DMs.")]
    public class TicketCommands : BaseCommandModule
    {
        private readonly TicketService _ticketService;
        public TicketCommands(TicketService ticketService) => _ticketService = ticketService;

        [Command]
        [RequireDirectMessage]
        [Description("Open a ticket for an issue, bug or other reason.")]
        public async Task Create(CommandContext ctx, string message = "No message provided")
        {
            
            TicketCreationResult? result = await _ticketService.CreateAsync(ctx.User, message).ConfigureAwait(false);
            if (!result.Succeeded)
            {
                await ctx.RespondAsync(result.Reason).ConfigureAwait(false);
                return;
            }
            Ticket ticket = result.Ticket;
            ulong channelId = TicketService.GetTicketChannel(ticket!.Opener); // If it succeeded, it's not null. //
            DiscordChannel ticketChannel = ctx.Client.Guilds[721518523704410202].GetChannel(channelId);
            DiscordEmbed embed = TicketEmbedHelper.GenerateInboundEmbed(message, ctx.User, ticket);
            await ticketChannel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        [Command]
        [RequireGuild]
        [Description("Close the ticket the current channel corresponds to.")]
        public async Task Close(CommandContext ctx)
        {
            await _ticketService.CloseTicket(ctx.Message);
        }

        [Command]
        [RequireGuild]
        public async Task Close(CommandContext ctx, ulong userId)
        {
            try { await _ticketService.CloseTicket(userId); }
            catch (InvalidOperationException e) { await ctx.RespondAsync(e.Message).ConfigureAwait(false); }
        }



    }
}