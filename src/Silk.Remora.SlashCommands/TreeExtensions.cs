using System.Text.RegularExpressions;
using OneOf;
using Remora.Commands.Trees;
using Remora.Commands.Trees.Nodes;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Commands.Extensions;
using static Remora.Discord.API.Abstractions.Objects.ApplicationCommandOptionType;

namespace Silk.Remora.SlashCommands;

/// <summary>
/// <para>
/// An implementation of extensions for <see cref="CommandTree"/> that behaves
/// similar to <see cref="CommandTreeExtensions"/>, but only registers commands
/// that are opt IN.
/// </para>
/// <remarks>
/// For the inclined, <see cref="CommandTreeExtensions"/> will attmept to register
/// all types listed in the command tree regardless of what they're marked with
/// because it makes the assumption that if it's a valid command class, and the
/// method is marked with the command attribute, it's intended to be a slash-
/// command. This is not the case for this extension, so it will only register
/// commands that are explicitly marked with an command type attribute, and will
/// NOT make an assumption of the type, as that can lead to ambiguity between
/// whether a command is meant to be a slash or text command at a glance.
/// </remarks>
/// </summary>
internal static class TreeExtensions
{
    // Taken directly from Remora because it was convinient to use.
    // Don't care what you say, I'm still terrified of a DMCA, Jax.
    
    private const int MaxRootCommandsOrGroups     = 100;
    private const int MaxGroupCommands            = 25;
    private const int MaxChoiceValues             = 25;
    private const int MaxCommandParameters        = 25;
    private const int MaxCommandStringifiedLength = 4000;
    private const int MaxChoiceNameLength         = 100;
    private const int MaxChoiceValueLength        = 100;
    private const int MaxCommandDescriptionLength = 100;
    private const int MaxTreeDepth                = 3; // Top level is a depth of 1
    private const int RootDepth                   = 1;
    
    private const string NameRegexPattern = "^[a-z0-9_-]{1,32}$";

    private static readonly Regex SlashRegex = new(NameRegexPattern, RegexOptions.Compiled);

    public static Dictionary<SlashCommandIdentifier, OneOf<CommandNode, Dictionary<string, CommandNode>>> 
        MapApplicationCommands
        (
            this CommandTree                   commandTree,
            IReadOnlyList<IApplicationCommand> commands
        )
    {
        var mapping = new Dictionary<SlashCommandIdentifier, OneOf<CommandNode, Dictionary<string, CommandNode>>>();

        foreach (var command in commands)
        {
            var isContextOrRootCommand = !command.Options.IsDefined(out var options) ||
                                         options.All(o => o.Type is not (SubCommand or SubCommandGroup));

            if (isContextOrRootCommand)
            {
                //Attempt to find the node in the tree.
                var commandNode = commandTree.Root.Children.OfType<CommandNode>()
                                             .FirstOrDefault(c =>
                                              {
                                                  if (!c.Key.Equals(command.Name, StringComparison.OrdinalIgnoreCase))
                                                      return false;

                                                  if (!command.Options.IsDefined(out var options))
                                                      return true;

                                                  // Please don't give two overloads the same named options :c
                                                  // This also should fix an issue that could be caused by having
                                                  // two overloads, one serving as a context menu command,
                                                  // and the other a slash, but I have no idea how likely that situation is.
                                                  return c.Shape.Parameters
                                                          .Select(p => p.Parameter.Name)
                                                          .SequenceEqual(options.Select(o => o.Name));
                                              });

                if (commandNode is null)
                    throw new InvalidOperationException("A command was not present in the command tree, but was present in the commands list, and cannot be mapped.");
                
                mapping.Add(new(command.GuildID, command.ID), commandNode);
                
                continue;
            }

            if (!command.Options.IsDefined(out options))
                throw new InvalidOperationException("A command was marked as a group, but did not contain any sub-commands.");

            foreach (var option in options)
            {
                
            }
        }
    }
    

    internal static IEnumerable<(List<string> Path, CommandNode Node)> MapOptions
        (
            IParentNode               parent,
            List<string>              outerPath,
            IApplicationCommandOption option
        )
    {
        
    }


}