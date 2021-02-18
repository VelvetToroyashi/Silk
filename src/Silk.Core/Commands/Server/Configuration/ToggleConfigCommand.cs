using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MediatR;

namespace Silk.Core.Commands.Server.Configuration
{
    [Group]
    public class ToggleConfigCommand
    {
        private readonly IMediator _mediator;

        public ToggleConfigCommand(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        public async Task Invites(CommandContext ctx)
        {
            
        }
    }
}