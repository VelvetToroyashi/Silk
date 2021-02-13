using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Core.Database.Models;
using Silk.Core.Services.Interfaces;
using Silk.Core.Services.Tickets;
using Silk.Core.Utilities;

namespace Silk.Core.Commands.General
{
    /// <summary>
    /// Class responsible for the creation of tickets.
    /// </summary>
    [Experimental]
    [Group("ticket")]
    [Category(Categories.Bot)]
    [Description("Commands related to tickets; opening tickets can only be performed in DMs.")]
    public class TicketCommands : BaseCommandModule
    {
        private readonly ITicketService _ticketService;
        public TicketCommands(ITicketService ticketService) => _ticketService = ticketService;

        [Command]
        [RequireDirectMessage]
        [Description("Open a ticket for an issue, bug or other reason.")]
        public async Task Create(CommandContext ctx, string message = "No message provided")
        {
            TicketCreationResult result = await _ticketService.CreateTicketAsync(ctx.User, message).ConfigureAwait(false);
            if (!result.Succeeded)
            {
                await ctx.RespondAsync(result.Reason).ConfigureAwait(false);
                return;
            }
            
            Ticket ticket = result.Ticket!;
            
            ulong channelId = await _ticketService.GetTicketChannelByUserId(ticket!.Opener); // If it succeeded, it's not null. //
            DiscordChannel ticketChannel = ctx.Client.Guilds[721518523704410202].GetChannel(channelId);
            DiscordEmbed embed = TicketEmbedHelper.GenerateInboundEmbed(message, ctx.User, ticket);
            
            await ticketChannel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        [Command]
        [RequireGuild]
        [Description("Close the ticket the current channel corresponds to.")]
        public async Task Close(CommandContext ctx)
        {
            await _ticketService.CloseTicketByChannelAsync(ctx.Message);
        }

        [Command]
        [RequireGuild]
        public async Task Close(CommandContext ctx, ulong userId)
        {
            try { await _ticketService.CloseTicketByUserIdAsync(userId); }
            catch (InvalidOperationException e) { await ctx.RespondAsync(e.Message).ConfigureAwait(false); }
        }



    }
}