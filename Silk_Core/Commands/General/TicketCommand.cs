#pragma warning disable CA1822 // Mark members as static
#pragma warning disable IDE0060 // Unused parameter
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Humanizer;
using Humanizer.Localisation;
using Microsoft.EntityFrameworkCore;
using Silk__Extensions;
using SilkBot.Database.Models;
using SilkBot.Extensions;
using SilkBot.Services;
using SilkBot.Utilities;

namespace SilkBot.Commands.General
{
    [Group]
    [Category(Categories.General)]
    public class Ticket : BaseCommandModule
    {
        private const string TERMINATION_REASON =
            "Your ticket has been manually terminated and is now void. No further information provided.";

        private const string TERMINATED_TICKET = "That ticket has been closed prior, and cannot be modified.";

        private const string TICKET_RECORD_MESSAGE =
            "Thank you for opening a ticket. For security reasons, conversation proxied though the bot is recorded. You will receive a response in due time.";

        private readonly Func<string, string> _ticketTermination =
            (r) => $"Your ticket has been terminated. Reason: `{r}`";

        private readonly Dictionary<ulong, ulong> _ticketChannels = new(); // UserId, ChannelId
        private readonly TicketService _ticketService;
        private readonly IDbContextFactory<SilkDbContext> _dbFactory;


        public Ticket(IDbContextFactory<SilkDbContext> dbFactory, TicketService ticket)
        {
            _ticketService = ticket;
            _dbFactory = dbFactory;
        }

        [Command("respond")]
        [Aliases("reply")]
        [RequireRoles(RoleCheckMode.Any, "Silk Contributer")]
        [RequireGuild()]
        public async Task RespondToTicket(CommandContext ctx, int Id, [RemainingText] string message)
        {
            using SilkDbContext db = _dbFactory.CreateDbContext();
            TicketModel? ticket = db.Tickets.OrderBy(t => t.Opened).LastOrDefault(ticket => ticket.Id == Id);
            if (ticket is not null)
            {
                if (!ticket.IsOpen)
                    await ctx.RespondAsync("That ticket has been closed!");
                else await _ticketService.RespondToTicket(ctx, message, ticket).ConfigureAwait(false);
            }
            else
            {
                await ctx.RespondAsync($"Ticket Id {Id} doesn't exist!");
            }
        }
        [RequireDirectMessage]
        [Command("create")]
        [Description("Create a ticket.")]
        
        public async Task OpenTicket(CommandContext ctx, [RemainingText] string messageContent)
        {
            TicketCreationResult ticket =
                await _ticketService.CreateTicketAsync(ctx.User, DateTime.Now, messageContent);
            if (ticket.Succeeded)
            {
                DiscordEmbedBuilder embed = _ticketService.GenerateRespondantEmbed(messageContent,
                    ctx.Client.CurrentUser.AvatarUrl, ctx.User, ticket.Ticket);
                DiscordChannel ticketChannel =
                    await _ticketService.GetOrCreateTicketChannelAsync(ctx.Client, ctx.User.Id);
                await ticketChannel.SendMessageAsync(embed: embed);
                await ctx.RespondAsync(TICKET_RECORD_MESSAGE);
                _ticketChannels.Add(ctx.User.Id, ticketChannel.Id);
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
            }

            else
            {
                await ctx.RespondAsync($"Your ticket could not be created due to: `{ticket.Reason}`");
            }
        }


        [Command("close")]
        [RequireRoles(RoleCheckMode.Any, "Silk Contributor")]
        [RequireGuild()]
        public async Task CloseTicket(CommandContext ctx, int id, [RemainingText] string reason = TERMINATION_REASON)
        {
            using SilkDbContext db = _dbFactory.CreateDbContext();
            TicketModel ticket = db.Tickets.SingleOrDefault(t => t.Id == id);
            if (ticket is null)
            {
                await ctx.RespondAsync("Invalid ticket id!");
            }
            else if (ctx.Channel.Id != _ticketChannels[ticket.Opener])
            {
                await SendInvalidTicketErrorAsync(ctx.Channel, ctx.Guild.GetChannel(_ticketChannels[ticket.Opener]));
            }
            else if (!ticket.IsOpen)
            {
                await ctx.RespondAsync(TERMINATED_TICKET);
            }
            else
            {
                ticket.Closed = DateTime.Now;
                ticket.IsOpen = false;

                await db.SaveChangesAsync();
                DiscordChannel c = await _ticketService.GetDmChannelAsync(ctx.Client, ticket.Opener);
                await c.SendMessageAsync(embed: EmbedHelper.CreateEmbed(ctx, _ticketTermination(reason),
                    DiscordColor.Red));
                _ticketChannels.Remove(ticket.Opener);
                await ctx.Channel.DeleteAsync();
            }
        }

        private static async Task SendInvalidTicketErrorAsync(DiscordChannel tc, DiscordChannel cc)
        {
            await tc.SendMessageAsync(
                $"Sorry but you can't close this ticket from this channel! Execute `<prefix>ticket close <id>` in {cc.Mention} instead.");
        }

        [Category(Categories.General)]
        public class ListTicketsCommand : CommandClass
        {
            public ListTicketsCommand(IDbContextFactory<SilkDbContext> db) : base(db) { }

            private readonly Func<CommandContext, DiscordUser, TicketModel, DiscordEmbedBuilder> _getTicketEmbed =
                (c, u, t) => new DiscordEmbedBuilder()
                             .WithAuthor($"{u.Username}#{u.Discriminator}", iconUrl: u.AvatarUrl)
                             .AddField("Opened by:", t.Opener.ToString())
                             .AddField("Opened on:",
                                 $"{t.Opened:d/M/yyyy} ({t.Opened.GetTime().Humanize(3, false, null, TimeUnit.Year, TimeUnit.Minute)} ago.)")
                             .AddField("Closed on:",
                                 t.IsOpen
                                     ? "This ticket is still active!"
                                     : $"{t.Closed:d/M/yyyy} ({t.Closed.GetTime().Humanize(3, false, null, TimeUnit.Year, TimeUnit.Minute)} ago.)")
                             .WithColor(DiscordColor.Chartreuse)
                             .WithFooter($"Silk! Requested by: {c.User.Id}", c.Client.CurrentUser.AvatarUrl)
                             .WithTimestamp(DateTime.Now);


            [Command("List")]
            [RequireRoles(RoleCheckMode.Any, "Silk Contributor")]
            [RequireGuild()]
            public async Task ListTickets(CommandContext ctx)
            {
                var db = new Lazy<SilkDbContext>();
                await ctx.RespondAsync("Please specify a name or ticket Id");
                InteractivityExtension interactivity = ctx.Client.GetInteractivity();
                InteractivityResult<DiscordMessage> msg =
                    await interactivity.WaitForMessageAsync(m => m.Author == ctx.Message.Author);
                if (msg.TimedOut)
                {
                    await ctx.RespondAsync("Timed out.");
                }
                else
                {
                    string msgContent = msg.Result.Content;
                    bool isNumber = Regex.IsMatch(msgContent, @"[0-9]+$");
                    if (isNumber)
                    {
                        TicketModel ticket = db.Value.Tickets.SingleOrDefault(t => t.Id == int.Parse(msgContent));
                        if (ticket is null)
                        {
                            await ctx.RespondAsync(
                                $"ArgumentOutOfRangeException: Ticket with Id `{msg.Result.Content}` does not exist.");
                        }
                        else
                        {
                            DiscordUser ticketOpener = await ctx.Client.GetUserAsync(ticket.Opener);
                            DiscordEmbed embed = _getTicketEmbed(ctx, ticketOpener, ticket);
                            await ctx.RespondAsync(embed: embed);
                        }
                    }
                    else
                    {
                        DiscordUser ticketMember = ctx.Client.GetUser(msg.Result.Content);
                        if (ticketMember is null)
                        {
                            await ctx.RespondAsync("Could not retrieve member by that name!");
                        }
                        else
                        {
                            TicketModel ticket = db.Value.Tickets.OrderBy(t => t.Opened)
                                                   .LastOrDefault(t => t.Opener == ticketMember.Id);
                            _ = ticket == null
                                ? await ctx.RespondAsync($"{ticketMember.Username} has no tickets!")
                                : await ctx.RespondAsync(embed: _getTicketEmbed(ctx, ticketMember, ticket));
                        }
                    }
                }
            }


            [Command("List")]
            [RequireRoles(RoleCheckMode.Any, "Silk Contributor")]
            [RequireGuild]
            public async Task ListTickets(CommandContext ctx, int Id)
            {
                using SilkDbContext db = GetDbContext();
                TicketModel ticket = db.Tickets.SingleOrDefault(t => t.Id == Id);
                if (ticket is null)
                {
                    await ctx.RespondAsync($"ArgumentOutOfRangeException: Ticket with Id `{Id}` does not exist.");
                }
                else
                {
                    DiscordUser ticketOpener = await ctx.Client.GetUserAsync(ticket.Opener);
                    DiscordEmbed embed = _getTicketEmbed(ctx, ticketOpener, ticket);
                    await ctx.RespondAsync(embed: embed);
                }
            }

            [Command("List")]
            [RequireRoles(RoleCheckMode.Any, "Silk Contributer")]
            [RequireGuild()]
            public async Task ListTickets(CommandContext ctx, string name)
            {
                using SilkDbContext db = GetDbContext();
                DiscordUser user = await new MemberSelectorService().SelectUser(ctx, ctx.GetUserByName(name));
                if (user is null)
                {
                    await ctx.RespondAsync("Canceled.");
                    return;
                }

                TicketModel ticket = db.Tickets.SingleOrDefault(t => t.Opener == user.Id);
                if (ticket is null)
                {
                    await ctx.RespondAsync($"{user.Username} has not opened any tickets.");
                }
                else
                {
                    DiscordEmbed embed = _getTicketEmbed(ctx, user, ticket);
                    await ctx.RespondAsync(embed: embed);
                }
            }
        }
    }
#nullable enable
    public struct TicketCreationResult
    {
        public bool Succeeded { get; }
        public string? Reason { get; }
        public TicketModel? Ticket { get; }

        public TicketCreationResult(bool s, string? r = default, TicketModel? t = default)
        {
            Succeeded = s;
            Reason = r;
            Ticket = t;
        }
    }
#nullable disable

    public class TicketService
    {
        private readonly DiscordShardedClient _client;
        private readonly IDbContextFactory<SilkDbContext> _dbFactory;

        public TicketService(DiscordShardedClient client, IDbContextFactory<SilkDbContext> dbFactory)
        {
            _client = client;
            _dbFactory = dbFactory;
        }

#nullable enable
        public TicketModel? GetTicketById(int Id)
        {
            using var db = _dbFactory.CreateDbContext();
            TicketModel? t = db.Tickets.SingleOrDefault(t => t.Id == Id);
            return t;
        }
#nullable disable


        public async Task<TicketCreationResult> CreateTicketAsync(DiscordUser ticketOpener, DateTime ticketTime,
            string ticketMessage)
        {
            using SilkDbContext db = _dbFactory.CreateDbContext();
            DbSet<TicketModel> tickets = db.Tickets;
            if (tickets.Where(t => t.IsOpen).Select(t => t.Opener).Contains(ticketOpener.Id))
                return new TicketCreationResult(false, "Ticket already opened for current user.");
            var ticket = new TicketModel() {IsOpen = true, Opener = ticketOpener.Id, Opened = ticketTime};
            ticket.History.Add(new TicketMessageHistoryModel {Message = ticketMessage, Sender = ticketOpener.Id});
            db.Tickets.Add(ticket);
            await db.SaveChangesAsync();
            return new TicketCreationResult(true, t: ticket);
        }

        public async Task<DiscordChannel> GetOrCreateTicketChannelAsync(DiscordClient c, ulong userId)
        {
            DiscordGuild g = c.Guilds[721518523704410202];
            Permissions perms = Permissions.SendMessages | Permissions.AccessChannels | Permissions.EmbedLinks;
            DiscordChannel parentCategory;
            if (!g.Channels.Values.Any(c => c.IsCategory && c.Name.ToLower() == "Silk! Tickets".ToLower()))
            {
                parentCategory = await g.CreateChannelCategoryAsync("Silk! Tickets");
                await parentCategory.AddOverwriteAsync(g.EveryoneRole, Permissions.None, Permissions.All);
                await parentCategory.AddOverwriteAsync(g.GetRole(745751916608356467), perms);
                await parentCategory.AddOverwriteAsync(g.GetRole(721514294587424888), perms);
            }
            else
            {
                parentCategory =
                    (await g.GetChannelsAsync()).Single(c => c.Name.ToLower() == "Silk! Tickets".ToLower());
            }

            DiscordUser u = _client.GetUser(userId);
            DiscordChannel returnChannel =
                g.Channels.Any(c => c.Value.Name.ToLower() == u.Username.ToLower())
                    ? g.Channels.Values.Single(c => c.Name.ToLower() == u.Username.ToLower())
                    : await g.CreateChannelAsync((await c.GetUserAsync(userId)).Username.ToLower(), ChannelType.Text,
                        parentCategory);
            return returnChannel;
        }

        public async Task RespondToBlindTicketAsync(DiscordClient client, ulong userId, string message)
        {
            using SilkDbContext db = _dbFactory.CreateDbContext();

            TicketModel ticket = db.Tickets.OrderBy(t => t.Opened).Last(ticket => ticket.Opener == userId);

            ticket.History.Add(new TicketMessageHistoryModel
                {Message = message, Sender = userId}); // Add this message to the history.
            await db.SaveChangesAsync();
            DiscordEmbedBuilder embed = GenerateRespondantEmbed(message, client.CurrentUser.AvatarUrl,
                await client.GetUserAsync(userId), ticket);
            DiscordChannel c = await GetOrCreateTicketChannelAsync(client, userId);
            await c.SendMessageAsync(embed: embed);
        }

        //private async ValueTask<DiscordChannel> GetTicketUserAsync(DiscordClient c, ulong Id)
        //{
        //    DiscordGuild g = c.Guilds[721518523704410202];
        //    DiscordUser u = await g.GetMemberAsync(Id);
        //    return g.Channels.Values.SingleOrDefault(c => c.Name == u.Username);
        //}


        public async Task RespondToTicket(CommandContext ctx, string message, TicketModel ticket)
        {
            DiscordMember recipient = await ctx.Client.Guilds
                                               .First(g => g.Value.Members
                                                            .Any(member => member.Key == ticket.Opener)).Value
                                               .GetMemberAsync(ticket.Opener);

            ticket.History.Add(new TicketMessageHistoryModel
                {Message = message, Sender = ctx.User.Id}); // Add this message to the history.
            await _dbFactory.CreateDbContext().SaveChangesAsync();
            DiscordEmbedBuilder embed =
                GenerateResponderEmbed(message, _client.CurrentUser.AvatarUrl, ctx.User, ticket);
            await FinalizeTicketPrecedure(ctx, recipient, embed);
        }


        private static async Task FinalizeTicketPrecedure(CommandContext ctx, DiscordMember recipient,
            DiscordEmbed embed)
        {
            await recipient.SendMessageAsync(embed: embed);
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
        }


        public DiscordEmbedBuilder GenerateRespondantEmbed(string message, string avatarUrl, DiscordUser ticketOpener,
            TicketModel ticket)
        {
            return new DiscordEmbedBuilder()
                   .WithAuthor(ticketOpener.Username, null, ticketOpener.AvatarUrl)
                   .WithColor(DiscordColor.DarkBlue)
                   .WithDescription(message)
                   .WithFooter($"Silk! | Ticket Id: {ticket.Id}", avatarUrl)
                   .WithTimestamp(DateTime.Now);
        }


        private DiscordEmbedBuilder GenerateResponderEmbed(string message, string avatarUrl, DiscordUser responder,
            TicketModel ticket)
        {
            return new DiscordEmbedBuilder()
                   .WithAuthor(responder.Username, null, responder.AvatarUrl)
                   .WithColor(DiscordColor.Green)
                   .WithDescription(message)
                   .WithFooter($"Silk!", avatarUrl)
                   .WithTimestamp(DateTime.Now);
        }

        public async Task<DiscordChannel> GetDmChannelAsync(DiscordClient c, ulong recipientId)
        {
            IReadOnlyDictionary<ulong, DiscordGuild> guilds = c.Guilds;
            DiscordGuild? guild =
                guilds.Values.FirstOrDefault(g => g.Members.Select(member => member.Key).Contains(recipientId));
            return await guild.Members.Values.First(m => m.Id == recipientId).CreateDmChannelAsync();
        }


        public bool CheckForTicket(DiscordChannel c, ulong Id)
        {
            if (!c.IsPrivate)
                return false;

            using SilkDbContext db = _dbFactory.CreateDbContext();
            TicketModel ticket = db.Tickets
                                   .Where(t => t.IsOpen)
                                   .OrderBy(t => t.Opened)
                                   .LastOrDefault(t => t.Opener == Id);

            return ticket is not null && ticket.IsOpen;
        }
    }
}