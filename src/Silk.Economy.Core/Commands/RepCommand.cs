using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MediatR;

namespace Silk.Economy.Core.Commands
{
	public class RepCommand : BaseCommandModule
	{
		private readonly IMediator _mediator;
		
		public RepCommand(IMediator mediator)
        {
            _mediator = mediator;
        }
		
		[Command]
		[Description("Give reputation to a user")]
		public async Task GiveRep(CommandContext ctx, [Description("The user to give reputation to")] DiscordMember member, [Description("The amount of reputation to give")] int amount)
		{
			
        }
        
		
		
	}
}