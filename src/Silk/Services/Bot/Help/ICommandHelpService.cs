using System.Threading.Tasks;
using Remora.Rest.Core;
using Remora.Results;

namespace Silk.Services.Bot.Help;

public interface ICommandHelpService
{
    /// <summary>
    /// Shows help for a specified command, or shows all top-level commands if no command is specified. 
    /// </summary>
    /// <param name="commandName">The name of the command to display help for.</param>
    /// <param name="treeName">The optional tree name to search through.</param>
    /// <returns>A result of the operation.</returns>
    Task<Result> ShowHelpAsync(Snowflake channelID, string? commandName = null, string? treeName = null);
}