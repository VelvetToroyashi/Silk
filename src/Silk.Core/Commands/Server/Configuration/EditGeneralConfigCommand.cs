using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.Logging;
using Silk.Core.Services.Interfaces;

namespace Silk.Core.Commands.Server.Configuration
{
    public partial class BaseConfigConfigCommand
    {
        public partial class BaseEditConfigCommand
        {
            [Group("general")]
            public class EditGeneralConfigCommand
            {
                private readonly IDatabaseService _db;
                private readonly ILogger<EditGeneralConfigCommand> _logger;

                public EditGeneralConfigCommand(IDatabaseService db, ILogger<EditGeneralConfigCommand> logger)
                {
                    _db = db;
                    _logger = logger;
                }
                
                [GroupCommand]
                public async Task EditConfig(CommandContext ctx)
                {
                    Command? cmd = ctx.CommandsNext.RegisteredCommands["help"];
                    CommandContext? context = ctx.CommandsNext.CreateContext(ctx.Message, null, cmd, "config edit moderation");
                    _ = ctx.CommandsNext.ExecuteCommandAsync(context);
                }
                
            }
        }
        
    }
}