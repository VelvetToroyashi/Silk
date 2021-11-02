using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MediatR;

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
			
		}

	}
}