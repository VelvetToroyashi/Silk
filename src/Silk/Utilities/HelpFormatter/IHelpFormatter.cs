using System.Collections.Generic;
using Remora.Commands.Trees.Nodes;
using Remora.Discord.API.Abstractions.Objects;

namespace Silk.Utilities.HelpFormatter;

/// <summary>
///     Represents a utility class for generating an embed to display in a help command.
/// </summary>
public interface IHelpFormatter
{
	/// <summary>
	///     Used to format a message representing a single command.
	/// </summary>
	/// <param name="command">The command to show a help message for.</param>
	/// <param name="overloads">Any overloads the command has, or null if it has none.</param>
	/// <returns>An embed displaying help for a single command.</returns>
	public IEmbed GetHelpEmbed(CommandNode command, IEnumerable<CommandNode>? overloads = null);

	/// <summary>
	///     Used to format a message representing a group of commands. Any of the nodes passed to the method may be a group node.
	/// </summary>
	/// <param name="subcommands">The subcommands of the parent.</param>
	/// <returns>An embed displaying help for a group of commands.</returns>
	public IEmbed GetHelpEmbed(IEnumerable<IChildNode> subcommands);
}