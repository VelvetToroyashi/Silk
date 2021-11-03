using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using MediatR;
using Silk.Economy.Data;
using Silk.Economy.Data.Models;
using Silk.Extensions;

namespace Silk.Economy.Core.Commands
{
	public class TransactionsCommand : BaseCommandModule
	{
		private readonly IMediator _mediator;
		public TransactionsCommand(IMediator mediator) => _mediator = mediator;

		[Command("transactions")]
		[Description("View your transactions")]
		public async Task TransactionsAsync(CommandContext ctx)
		{
			var transactions = (await _mediator.Send(new GetTransactions.Request(ctx.User.Id))).ToList();
			
			if (transactions.Count is 0)
		    {
		        await ctx.RespondAsync("You have no transactions.");
		        return;
		    }

			var embed = new DiscordEmbedBuilder()
				.WithTitle("Transactions")
				.WithColor(DiscordColor.Green);

			var transList = transactions
				.ChunkBy(5)
				.Select(trans => 
						trans
						.Select(t => GetTransactionString(t, ctx.User.Id))
						.Join("\n"));

			var interactivity = ctx.Client.GetInteractivity();
			var pages = transList.Select(trans => new Page(null, new DiscordEmbedBuilder(embed).WithDescription(trans)));
			
			await interactivity.SendPaginatedMessageAsync(ctx.Channel, ctx.User, pages, PaginationBehaviour.WrapAround, ButtonPaginationBehavior.Disable);
		}


		private string GetTransactionString(EconomyTransaction t, ulong id)
			=> $"${t.Amount} on {Formatter.Timestamp(t.Timestamp, TimestampFormat.ShortDate)}: " +
			   $"\n {(t.FromId == id ? "You" : t.FromId is 0 ? "**SYSTEM**" : $"<@{t.FromId}")} → {(t.ToId == id ? "You" : t.ToId is 0 ? "**SYSTEM**" : $"<@{t.ToId}")}";
	}
}