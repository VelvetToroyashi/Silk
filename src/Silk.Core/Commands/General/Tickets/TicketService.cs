using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Silk.Core.Database;
using Silk.Core.Database.Models;

namespace Silk.Core.Commands.General.Tickets
{
    public class TicketService
    {
        private readonly DiscordShardedClient _client;
        private readonly ILogger<TicketService> _logger;
        private readonly IDbContextFactory<SilkDbContext> _dbFactory;

        private const ulong SILK_GUILD_ID = 721518523704410202;
        private const ulong SILK_CONTRIBUTOR_ID = 745751916608356467;


        // private readonly DatabaseService _dbService;
        public TicketService(DiscordShardedClient client, ILogger<TicketService> logger, IDbContextFactory<SilkDbContext> dbFactory)
        {
            _client = client;
            _logger = logger;
            _dbFactory = dbFactory;
        }

        // Hold a dictionary of currently open ticket channels. //
        // UserId, ChannelId //
        private static readonly ConcurrentDictionary<ulong, ulong> ticketChannels = new();
        private static readonly Permissions requisitePermissions = Permissions.SendMessages | Permissions.AccessChannels |
                                                                   Permissions.EmbedLinks | Permissions.ReadMessageHistory;

        /// <summary>
        /// Create a TicketModel in the database, and add the message sent as the first piece of history for it. 
        /// </summary>
        /// <param name="user">The user that opened the ticket.</param>
        /// <param name="message">The message they sent.</param>
        /// <returns>Result of creating the ticket, indicating success or failure.</returns>
        public async Task<TicketCreationResult> CreateAsync(DiscordUser user, string message)
        {
            DiscordChannel ticketCategory = await GetOrCreateTicketCategoryAsync();
            SilkDbContext db = _dbFactory.CreateDbContext();

            // Make sure they don't have an open ticket already. //
            if (await db.Tickets.AnyAsync(t => t.Opener == user.Id && t.IsOpen))
                return new TicketCreationResult(false, "You have an active ticket already!", null);
            _logger.LogDebug($"Created ticket for {user.Username}#{user.Discriminator}.");

            // Database section. //
            var ticket = new Ticket {IsOpen = true, Opened = DateTime.Now, Opener = user.Id};
            await AddHistoryAsync(message, user.Id, ticket);
            await db.Tickets.AddAsync(ticket);
            await db.SaveChangesAsync();

            // Make sure the bot is on Silk! Official. //
            if (!TryGetSilkGuild(out DiscordGuild silk))
            {
                throw new KeyNotFoundException("Tickets aren't supported on self-hosted builds :(");
            }

            // Create the channel and add it to the dictionary //
            DiscordChannel c = await silk.CreateChannelAsync(user.Username, ChannelType.Text, ticketCategory,
                $"Ticket opened by {user.Username} on {DateTime.Now}");
            await c.AddOverwriteAsync(c.Guild.GetRole(SILK_CONTRIBUTOR_ID), requisitePermissions);

            ticketChannels.TryAdd(user.Id, c.Id);

            return new TicketCreationResult(true, null, ticket);
        }

        /// <summary>
        /// Add a message to a ticket's history.
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="user">The user that sent the message</param>
        /// <param name="ticket">The ticket to attach the message to</param>
        /// <returns></returns>
        public async Task AddHistoryAsync(string message, ulong user, Ticket ticket)
        {
            ticket.History.Add(new TicketMessage {Message = message, Sender = user, Ticket = ticket});
            await using var db = _dbFactory.CreateDbContext();
            db.Attach(ticket);
            await db.SaveChangesAsync();
        }


        private bool IsOpenTicket(ulong id) => _dbFactory
            .CreateDbContext()
            .Tickets
            .Where(t => t.Opener == id)
            .OrderBy(t => t.Opened)
            .Last()
            .IsOpen;

        public async Task CloseTicket(DiscordMessage message)
        {
            try
            {
                ulong userId = GetTicketUser(message.Channel);
                await using SilkDbContext db = _dbFactory.CreateDbContext();
                Ticket ticket = await GetTicketAsync(userId, db);
                ticket.Closed = DateTime.Now;
                ticket.IsOpen = false;
                await message.Channel.DeleteAsync();
                await db.SaveChangesAsync();
                ticketChannels.TryRemove(userId, out _);
                IEnumerable<KeyValuePair<ulong, DiscordMember>> members = _client.ShardClients.Values.SelectMany(s => s.Guilds.Values).SelectMany(g => g.Members);
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

        public async Task CloseTicket(ulong id)
        {
            if (!IsOpenTicket(id)) throw new InvalidOperationException("Not a ticket channel!");
            Ticket ticket = await GetTicketAsync(id);
            ticket.Closed = DateTime.Now;
            ticket.IsOpen = false;
            DiscordChannel ticketCategory = await GetOrCreateTicketCategoryAsync();
            await ticketCategory.Children.SingleOrDefault(c => c.Id == ticketChannels[id])!.DeleteAsync();
            ticketChannels.TryRemove(id, out _);
        }

        public async Task CloseTicketById(int ticketId)
        {
            SilkDbContext db = _dbFactory.CreateDbContext();

        }


        public async Task<Ticket> GetTicketAsync(ulong id)
        {
            SilkDbContext db = _dbFactory.CreateDbContext();
            return await db.Tickets.FirstOrDefaultAsync(t => t.IsOpen && t.Opener == id);
        }

        public async Task<Ticket> GetTicketAsync(ulong id, SilkDbContext dbContext) => await dbContext.Tickets.FirstOrDefaultAsync(t => t.IsOpen && t.Opener == id);

        /// <summary>
        /// Get a <see cref="DiscordUser"/>'s Id from the corresponding ticket channel.
        /// </summary>
        /// <param name="channel">The channel to search for.</param>
        /// <returns>The corresponding user the ticket channel belongs to.</returns>
        /// <exception cref="KeyNotFoundException">The channel Id is not a valid ticket channel.</exception>
        public static ulong GetTicketUser(DiscordChannel channel)
        {
            foreach ((ulong k, ulong v) in ticketChannels)
                if (v == channel.Id) return k;
            
            throw new KeyNotFoundException("Invalid ticket channel.");
        }

        public static ulong GetTicketChannel(ulong userId)
        {
            ticketChannels.TryGetValue(userId, out ulong channelId);
            return channelId;
        }

        public async Task<bool> HasTicket(DiscordChannel c, ulong id)
        {
            // If the message is sent on a guild, check if it's a ticket channel, else follow normal check flow. //
            if (!c.IsPrivate)
                return ticketChannels.ContainsKey(id);

            SilkDbContext db = _dbFactory.CreateDbContext();

            Ticket? ticket = await db.Tickets
                .Where(t => t.IsOpen)
                .OrderBy(t => t.Opened)
                .LastOrDefaultAsync(t => t.Opener == id);
            return ticket?.IsOpen ?? false;
        }

        private async ValueTask<DiscordChannel> GetOrCreateTicketCategoryAsync()
        {
            if (!TryGetSilkGuild(out DiscordGuild? silk))
                throw new KeyNotFoundException("Tickets aren't supported on self-hosted builds :(");
            

            DiscordChannel? ticketCategory = silk!.Channels.Values.FirstOrDefault(x => x.Name == "Silk! Tickets" && x.IsCategory);

            if (ticketCategory is not null) return ticketCategory;


            ticketCategory = await silk.CreateChannelCategoryAsync("Silk! Tickets");

            await ticketCategory.AddOverwriteAsync(silk.GetRole(SILK_CONTRIBUTOR_ID), requisitePermissions);
            await ticketCategory.AddOverwriteAsync(silk.EveryoneRole, Permissions.None, requisitePermissions);

            return ticketCategory;
        }

        private bool TryGetSilkGuild(out DiscordGuild? guild)
        {
            IEnumerable<DiscordGuild> guilds = _client.ShardClients.Values.SelectMany(g => g.Guilds.Values);
            guild = guilds.FirstOrDefault(g => g.Id == SILK_GUILD_ID);
            return guild != null;
        }

    }
}