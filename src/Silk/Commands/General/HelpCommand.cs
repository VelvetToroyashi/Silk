using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.Commands.Contexts;
using Remora.Results;
using Silk.Infrastructure;
using Silk.Services.Bot.Help;

namespace VTP.Remora.Commands.HelpSystem;

[ExcludeFromCodeCoverage]
public class HelpCommand : CommandGroup
{
    private readonly ICommandContext _context;
    private readonly ICommandHelpService _commandHelp; 
    private readonly IOptions<HelpSystemOptions> _options;
    
    
    public HelpCommand(ICommandContext context, ICommandHelpService commandHelp, IOptions<HelpSystemOptions> options)
    {
        _context     = context;
        _commandHelp = commandHelp;
        _options     = options;
    }

    [Command("help")]
    [Description("Displays help for a command or group of commands.")]
    public Task<Result> ShowHelpAsync([Greedy] string? command = null)
        => _commandHelp.ShowHelpAsync(_context.ChannelID, command, _options.Value.TreeName);

}