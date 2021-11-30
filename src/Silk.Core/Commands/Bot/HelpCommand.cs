using System.ComponentModel;
using System.Threading.Tasks;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Results;
using Silk.Core.Utilities.HelpFormatter;

namespace Silk.Core.Commands.Bot
{
    [HelpCategory(Categories.Bot)]
    public class HelpCommand : CommandGroup
    {
        private readonly ICommandContext   _context;
        private readonly CommandHelpViewer _help;
        public HelpCommand(CommandHelpViewer help, ICommandContext context)
        {
            _help = help;
            _context = context;
        }

        [Command("help")]
        [Description("Shows help for a command or group of commands.")]
        public Task<Result<IMessage>> Help([Greedy] [Description("View help about a command. Omit to show all commands.")] string? command = null)
        {
            return _help.SendHelpAsync(command, _context.ChannelID);
        }
    }
}