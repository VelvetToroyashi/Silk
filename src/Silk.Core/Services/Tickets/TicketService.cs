using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Silk.Core.Database;
using Silk.Core.Database.Models;
using Silk.Core.Services.Interfaces;

namespace Silk.Core.Services.Tickets
{
    public class TicketService : ITicketService, IConfiguredService
    {
        private readonly DiscordShardedClient _client;
        private readonly ILogger<TicketService> _logger;
        private readonly IDbContextFactory<SilkDbContext> _dbFactory;

        private const ulong SilkGuildId = 721518523704410202;
        private const ulong SilkContributorId = 745751916608356467;

        private const Permissions RequisitePermissions = Permissions.SendMessages | Permissions.AccessChannels |
                                                         Permissions.EmbedLinks | Permissions.ReadMessageHistory;
        
        public TicketService(DiscordShardedClient client, ILogger<TicketService> logger, IDbContextFactory<SilkDbContext> dbFactory)
        {
            _client = client;
            _logger = logger;
            _dbFactory = dbFactory;
        }

        /// <summary>
        /// Create a TicketModel in the database, and add the message sent as the first piece of history for it. 
        /// </summary>
        /// <param name="user">The user that opened the ticket.</param>
        /// <param name="message">The message they sent.</param>
        /// <returns>Result of creating the ticket, indicating success or failure.</returns>
        public async Task<TicketCreationResult> CreateTicketAsync(DiscordUser user, string message)
        {
            // Make sure the bot is on Silk! Official. //
            if (!TryGetSilkGuild(out DiscordGuild? silk))
            {
                throw new KeyNotFoundException("Tickets aren't supported on self-hosted builds :(");
            }

            DiscordChannel ticketCategory = await GetOrCreateTicketCategoryAsync();
            SilkDbContext db = _dbFactory.CreateDbContext();

            // Make sure they don't have an open ticket already. //
            if (await UserHasAnOpenTicketAsync(user, db))
                return new TicketCreationResult(false, "You have an active ticket already!", null);
            
            // Create the channel and add it to the dictionary //
            DiscordChannel ticketChannel = await silk!.CreateChannelAsync(user.Username, ChannelType.Text, ticketCategory,
                $"Ticket opened by {user.Username} on {DateTime.Now}");

            await ticketChannel.AddOverwriteAsync(ticketChannel.Guild.GetRole(SilkContributorId), RequisitePermissions);

            // Database section. //
            var ticket = new Ticket
            {
                IsOpen = true, 
                Opened = DateTime.Now, 
                Opener = user.Id, 
                ChannelId = ticketChannel.Id
            };
            
            await AddMessageToTicketHistoryAsync(message, user.Id, ticket);

            await db.Tickets.AddAsync(ticket);
            await db.SaveChangesAsync();

            _logger.LogDebug($"Created ticket for {user.Username}#{user.Discriminator}.");

            return new TicketCreationResult(true, null, ticket);
        }

        public async Task CloseTicketByUserIdAsync(ulong userId)
        {
            if (!UserHasAnOpenTicket(userId)) 
                throw new InvalidOperationException("No tickets were found to be able to close");

            Ticket ticket = await GetTicketByUserIdAsync(userId);
            ticket.Closed = DateTime.Now;
            ticket.IsOpen = false;

            DiscordChannel ticketCategory = await GetOrCreateTicketCategoryAsync();
            
            // Find the channel to delete based on the userId
            // Will prob need to query the Db based on the userId to get the associated channel
            await ticketCategory.Children.SingleOrDefault(c => c.Id == ticket.ChannelId)!.DeleteAsync();
        }

        public async Task CloseTicketByChannelAsync(DiscordMessage message)
        {
            try
            {
                // Get userId based off of the channel, since the Ticket holds the
                // userId and the associated Channel
                ulong userId = await GetUserIdByChannel(message.Channel);
                await using SilkDbContext db = _dbFactory.CreateDbContext();

                Ticket ticket = await GetTicketAsync(userId, db);
                ticket.Closed = DateTime.Now;
                ticket.IsOpen = false;

                await message.Channel.DeleteAsync();

                await db.SaveChangesAsync();

                IEnumerable<KeyValuePair<ulong, DiscordMember>> members = _client.ShardClients
                    .Values
                    .SelectMany(s => s.Guilds.Values)
                    .SelectMany(g => g.Members);

                DiscordMember member = members.SingleOrDefault(m => m.Key == userId)!.Value;

                await member.SendMessageAsync(TicketEmbedHelper.GenerateTicketClosedEmbed());
            }
            catch
            {
                var builder = new DiscordMessageBuilder()
                    .WithContent("This isn't a ticket channel!")
                    .WithReply(message.Id, true);

                await message.Channel.SendMessageAsync(builder);
            }
        }

        public async Task<Ticket> GetTicketByUserIdAsync(ulong userId)
        {
            SilkDbContext db = _dbFactory.CreateDbContext();
            return await db.Tickets.FirstOrDefaultAsync(t => t.IsOpen && t.Opener == userId);
        }
        
        public async Task<bool> UserHasOpenTicketAsync(ulong userId)
        {
            // If the message is sent on a guild, check if it's a ticket channel, else follow normal check flow. //
            SilkDbContext db = _dbFactory.CreateDbContext();

            Ticket? ticket = await db.Tickets
                .Where(t => t.IsOpen)
                .OrderBy(t => t.Opened)
                .LastOrDefaultAsync(t => t.Opener == userId);

            return ticket?.IsOpen ?? false;
        }

        private async Task<Ticket> GetTicketAsync(ulong userId, SilkDbContext dbContext) =>
            await dbContext.Tickets.FirstOrDefaultAsync(t => t.Opener == userId && t.IsOpen);

        private bool UserHasAnOpenTicket(ulong userId)
        {
            return _dbFactory
                .CreateDbContext()
                .Tickets
                .Where(t => t.Opener == userId)
                .OrderBy(t => t.Opened)
                .Last()
                .IsOpen;
        }

        private async Task<DiscordChannel> GetOrCreateTicketCategoryAsync()
        {
            if (!TryGetSilkGuild(out DiscordGuild? guild))
                throw new KeyNotFoundException("Tickets aren't supported on self-hosted builds :(");

            DiscordChannel? ticketCategory =
                guild!.Channels.Values.FirstOrDefault(x => x.Name == "Silk! Tickets" && x.IsCategory);

            if (ticketCategory is not null) return ticketCategory;

            ticketCategory = await guild.CreateChannelCategoryAsync("Silk! Tickets");

            await ticketCategory.AddOverwriteAsync(guild.GetRole(SilkContributorId), RequisitePermissions);
            await ticketCategory.AddOverwriteAsync(guild.EveryoneRole, Permissions.None, RequisitePermissions);

            return ticketCategory;
        }

        private bool TryGetSilkGuild(out DiscordGuild? guild)
        {
            IEnumerable<DiscordGuild> guilds = _client.ShardClients.Values.SelectMany(g => g.Guilds.Values);
            guild = guilds.FirstOrDefault(g => g.Id == SilkGuildId);
            return guild != null;
        }

        /// <summary>
        /// Add a message to a ticket's history.
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="user">The user that sent the message</param>
        /// <param name="ticket">The ticket to attach the message to</param>
        /// <returns></returns>
        private async Task AddMessageToTicketHistoryAsync(string message, ulong user, Ticket ticket)
        {
            ticket.History.Add(new TicketMessage {Message = message, Sender = user, Ticket = ticket});

            await using var db = _dbFactory.CreateDbContext();

            db.Attach(ticket);

            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Get a <see cref="DiscordUser"/>'s Id from the corresponding ticket channel.
        /// </summary>
        /// <param name="channel">The channel to search for.</param>
        /// <returns>The corresponding user the ticket channel belongs to.</returns>
        /// <exception cref="KeyNotFoundException">The channel Id is not a valid ticket channel.</exception>
        public async Task<ulong> GetUserIdByChannel(DiscordChannel channel)
        {
            await using var db = _dbFactory.CreateDbContext();
            Ticket? ticket = await db.Tickets.FirstOrDefaultAsync(t => t.ChannelId == channel.Id);
            
            if (ticket is null) 
                throw new KeyNotFoundException("Invalid ticket channel.");

            return ticket.Opener;
        }

        public async Task<ulong> GetTicketChannelByUserId(ulong userId)
        {
            await using var db = _dbFactory.CreateDbContext();
            Ticket? ticket = await db.Tickets.FirstOrDefaultAsync(t => t.Opener == userId);
            
            if (ticket is null) 
                throw new KeyNotFoundException("Invalid ticket channel.");

            return ticket.ChannelId;
        }

        private static async Task<bool> UserHasAnOpenTicketAsync(DiscordUser user, SilkDbContext db)
        {
            return await db.Tickets.AnyAsync(t => t.Opener == user.Id && t.IsOpen);
        }

        bool IConfiguredService.HasConfigured { get; set; }

        public Task Configure()
        {
            throw new NotImplementedException();
        }
    }
}