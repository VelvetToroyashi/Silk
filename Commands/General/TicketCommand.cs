using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using SilkBot.Database.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using static SilkBot.Bot;

namespace SilkBot
{
    public partial class Ticket : BaseCommandModule
    {

        [Command("Ticket")]
        public async Task TicketHandler(CommandContext ctx, [RemainingText] string messageContent)
        {
            var splitMessage = messageContent.Split(' ');
            switch (splitMessage[0])
            {
                case "respond":
                case "reply":
                    await RespondToTicket(ctx);
                    break;
                default:
                    await OpenTicket(messageContent);
                    break;

            }
        }

        private async Task RespondToTicket(CommandContext ctx)
        {
            var split = ctx.Message.Content.Split(' ');
            if (!int.TryParse(split[2], out var Id))
            {
                await ctx.RespondAsync("I couldn't figure out what Id you're trying to access!");
                return;
            }
            var ticket = GetTicketById(Id);
            if (!ticket.IsOpen)
            {
                await ctx.RespondAsync("That ticket has been closed!");
                return;
            }
            ticket.History.Add(new TicketMessageHistoryModel { Message = string.Join(' ', split[3..^0]), Sender = ctx.User.Id });
            var ticketOpener = await Instance.Client.Guilds.First(g => g.Value.Members.Any(member => member.Key == ticket.Opener)).Value.GetMemberAsync(ticket.Opener);
            var embed = new DiscordEmbedBuilder()
                .WithAuthor(ctx.User.Username, null, ctx.User.AvatarUrl)
                .WithColor(DiscordColor.Blurple)
                .WithDescription(string.Join(' ', split[3..^0]))
                .WithFooter("Silk!", ctx.Client.CurrentUser.AvatarUrl)
                .WithTimestamp(DateTime.Now);
            await ticketOpener.SendMessageAsync(embed: embed);

        }
        private async Task OpenTicket(string messageContent)
        {

        }

        private TicketModel GetTicketById(int Id)
        {
            return Instance.SilkDBContext.Tickets.OrderBy(t => t.Opened).Last(ticket => ticket.Id == Id);
        }

        private async Task<DiscordChannel> GetDmChannelAsync(ulong recipientId)
        {
            var guilds = Instance.Client.Guilds;
            var guild = guilds.Values.AsQueryable().FirstOrDefault(g => g.Members.Select(member => member.Key).Contains(recipientId));
            return await guild.Members.Values.First(m => m.Id == recipientId).CreateDmChannelAsync();
        }



        private DiscordEmbedBuilder GenerateTicketEmbed(string message, string avatarUrl, DiscordUser ticketOpener, TicketModel ticket)
        {
            return new DiscordEmbedBuilder()
                .WithAuthor(ticketOpener.Username, null, ticketOpener.AvatarUrl)
                .WithDescription(message)
                .WithFooter($"Silk! | Ticket Id: {ticket.Id}", avatarUrl)
                .WithTimestamp(DateTime.Now);
        }
        private async Task<TicketModel> GenerateTicketAsync(DbSet<TicketModel> tickets, DateTime openTime, ulong openerId)
        {
            tickets.Add(new TicketModel { Id = tickets.Count() + 1, IsOpen = true, Opened = openTime, Opener = openerId });
            await Instance.SilkDBContext.SaveChangesAsync();
            return tickets.AsQueryable().OrderBy(_ => _!.Opened).Last(ticket => ticket.Opener == openerId);
        }
    }
}
