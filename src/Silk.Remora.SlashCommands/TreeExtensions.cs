using System.Reflection;
using System.Text.RegularExpressions;
using OneOf;
using Remora.Commands.Extensions;
using Remora.Commands.Signatures;
using Remora.Commands.Trees;
using Remora.Commands.Trees.Nodes;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Results;
using Remora.Rest.Extensions;
using Remora.Results;
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
                var subCommands = MapOptions(commandTree.Root, new() { command.Name }, option);
                
                if (!mapping.TryGetValue(new (command.GuildID, command.ID), out var value))
                {
                    var subMap = new Dictionary<string, CommandNode>();
                    mapping.Add(new(command.GuildID, command.ID), subMap);
                    value = subMap;
                }

                var groupMapping = value.AsT1;
                
                foreach (var (path, subOption) in subCommands)
                    groupMapping.Add(string.Join("::", path), subOption);
            }
        }

        return mapping;
    }


    public static Result<IReadOnlyList<IBulkApplicationCommandData>> CreateApplicationCommands
        (
            this CommandTree tree
        )
    {
        var commands     = new List<BulkApplicationCommandData>();
        var commandNames = new Dictionary<int, HashSet<string>>();

        foreach (var node in tree.Root.Children)
        {
            var optionResult = GetCommandOptionFromNode(node, RootDepth);
            
            if (!optionResult.IsSuccess)
                return Result<IReadOnlyList<IBulkApplicationCommandData>>.FromError(optionResult.Error);

            if (!optionResult.IsDefined(out var option))
                continue;
            
            //NOTE: This is safe to do here becasue we've already validated that the node is a command that has been explicitly defined as one.
            var type = node is CommandNode command ? (int)command.GetCommandType() : -1;
            if (!commandNames.TryGetValue(type, out var names))
            {
                names = new HashSet<string>();
                commandNames.Add(type, names);
            }
            
            if (!names.Add(option.Name))
                return Result<IReadOnlyList<IBulkApplicationCommandData>>.FromError(new UnsupportedFeatureError("[Sub-]command [groups] do not support overloads.", node));
            
            var commandStringLength = (int)typeof(CommandTreeExtensions)
                                          .GetMethod("GetCommandStringifiedLength", BindingFlags.NonPublic | BindingFlags.Static)?
                                          .Invoke(null, new object[] { node })!;
            
            if (commandStringLength > MaxCommandStringifiedLength)
                return Result<IReadOnlyList<IBulkApplicationCommandData>>
                   .FromError(
                              new UnsupportedFeatureError("One or more commands is too long (combined length of name," +
                                                          $" description, and value properties), max {MaxCommandStringifiedLength}).", node));
            
            
            
        }

    }


    private static Result<IApplicationCommandOption?> GetCommandOptionFromNode(IChildNode node, int treeDepth)
    {
        if (treeDepth > MaxTreeDepth)
            return new ArgumentOutOfRangeError($"A sub-command or group was nested too deeply. Max: {MaxTreeDepth}, Current: {treeDepth}");

        switch (node)
        {
            case CommandNode command:
            {
                if (CustomAttributeProviderExtensions.GetCustomAttribute<ExcludeFromSlashCommandsAttribute>(command.GroupType) is not null)
                    return Result<IApplicationCommandOption?>.FromSuccess(null);

                if (CustomAttributeProviderExtensions.GetCustomAttribute<ExcludeFromSlashCommandsAttribute>(command.CommandMethod) is not null)
                    return Result<IApplicationCommandOption?>.FromSuccess(null);

                var validateNameResult = (Result)typeof(CommandTreeExtensions).GetMethod("ValidateNodeName", BindingFlags.Static | BindingFlags.NonPublic)!
                                                                              .Invoke(null, new object[] { command.Key, command })!;
                if (!validateNameResult.IsSuccess)
                    return Result<IApplicationCommandOption?>.FromError(validateNameResult);

                var validateDescriptionResult = (Result)typeof(CommandTreeExtensions).GetMethod("ValidateNodeDescription", BindingFlags.Static | BindingFlags.NonPublic)!
                                                                                     .Invoke(null, new object[] { command.Shape.Description, command })!;
                if (!validateDescriptionResult.IsSuccess)
                    return Result<IApplicationCommandOption?>.FromError(validateDescriptionResult);

                var commandType = CustomAttributeProviderExtensions.GetCustomAttribute<CommandTypeAttribute>(command.CommandMethod)?.Type;

                if (commandType is null) // If it isn't explicitly marked, it's not a slash command.
                    return Result<IApplicationCommandOption?>.FromSuccess(null);

                if (commandType is not ApplicationCommandType.ChatInput)
                {
                    if (treeDepth > RootDepth)
                        return new UnsupportedFeatureError("Context-Menus may not be nested.", command);

                    if (command.CommandMethod.GetParameters().Any())
                        return new UnsupportedFeatureError("Context-Menus may not have parameters.", command);
                }

                var buildOptionsResult = (Result<IReadOnlyList<IApplicationCommandOption>>)typeof(CommandTreeExtensions).GetMethod("CreateCommandParameterOptions", BindingFlags.Static | BindingFlags.NonPublic)!
                                                                                                                        .Invoke(null, new object[] { command })!;

                if (!buildOptionsResult.IsSuccess)
                    return Result<IApplicationCommandOption?>.FromError(buildOptionsResult.Error);

                var key = commandType is not ApplicationCommandType.ChatInput
                    ? command.Key
                    : command.Key.ToLowerInvariant();

                return new ApplicationCommandOption
                    (
                     SubCommand, // Might not actually be a sub-command, but the caller will handle that
                     key,
                     command.Shape.Description,
                     Options: new(buildOptionsResult.Entity)
                    );
            }
            case GroupNode group:
            {
                var validateNameResult = (Result)typeof(CommandTreeExtensions).GetMethod("ValidateNodeName", BindingFlags.Static | BindingFlags.NonPublic)!
                                                                              .Invoke(null, new object[] { group.Key, group })!;

                if (!validateNameResult.IsSuccess)
                    return Result<IApplicationCommandOption?>.FromError(validateNameResult);

                var validateDescriptionResult = (Result)typeof(CommandTreeExtensions).GetMethod("ValidateNodeDescription", BindingFlags.Static | BindingFlags.NonPublic)!
                                                                                     .Invoke(null, new object[] { group.Description, group })!;

                if (!validateDescriptionResult.IsSuccess)
                    return Result<IApplicationCommandOption?>.FromError(validateDescriptionResult);

                var groupOptions     = new List<IApplicationCommandOption>();
                var groupOptionNames = new HashSet<string>();
                var subcommands      = 0;

                foreach (var child in group.Children)
                {
                    var optionResult = GetCommandOptionFromNode(child, treeDepth + 1);

                    if (!optionResult.IsSuccess)
                        return Result<IApplicationCommandOption?>.FromError(optionResult.Error);

                    if (!optionResult.IsDefined(out var option))
                        continue;

                    if (option.Type is SubCommand)
                        ++subcommands;

                    if (!groupOptionNames.Add(option.Name))
                        return new UnsupportedFeatureError("A group may not contain multiple options with the same name.", group);

                    groupOptions.Add(option);
                }

                if (!groupOptions.Any())
                    return Result<IApplicationCommandOption?>.FromSuccess(null);

                if (subcommands > MaxGroupCommands)
                    return new UnsupportedFeatureError($"Subcommands ({subcommands}) in group exceeded maximum of {MaxGroupCommands}.");


                return new ApplicationCommandOption
                    (
                     SubCommandGroup,
                     group.Key.ToLowerInvariant(),
                     group.Description,
                     Options: new(groupOptions)
                    );
            }
            default:
                throw new InvalidOperationException($"Unable to trnaslate node of {node.GetType().Name} into an application command.");
        }
    }
    
    private static ApplicationCommandOptionType ToApplicationCommandOptionType(Type parameterType)
    {
        var discordType = parameterType switch
        {
            var t when t == typeof(bool)         => ApplicationCommandOptionType.Boolean,
            var t when t == typeof(IRole)        => ApplicationCommandOptionType.Role,
            var t when t == typeof(IUser)        => ApplicationCommandOptionType.User,
            var t when t == typeof(IGuildMember) => ApplicationCommandOptionType.User,
            var t when t == typeof(IChannel)     => ApplicationCommandOptionType.Channel,
            var t when t.IsInteger()             => Integer,
            var t when t.IsFloatingPoint()       => Number,
            var t when t == typeof(IAttachment)  => (ApplicationCommandOptionType)11, //Attachment,
            _                                    => ApplicationCommandOptionType.String
        };

        return discordType;
    }
    
    internal static IEnumerable<(List<string> Path, CommandNode Node)> MapOptions
        (
            IParentNode               parent,
            List<string>              outerPath,
            IApplicationCommandOption option
        )
    {
        //Regardless of the type we essentially traverse the tree.
        if (option.Type is SubCommand)
        {
            var path  = outerPath[0];
            var depth = 0;

            var current = parent;
            
            // Keep traversing until we've found the 'leaf' node.
            while (true)
            {
                var pathFoundNode = current.Children.FirstOrDefault(c => c.Key.Equals(path ?? option.Name, StringComparison.OrdinalIgnoreCase));
                
                if (pathFoundNode is null)
                    throw new InvalidOperationException("A sub-command was not present in the command tree, but was present in the options list, and cannot be mapped.");

                if (pathFoundNode is IParentNode group)
                {
                    ++depth;

                    path    = outerPath.Skip(depth).FirstOrDefault();
                    current = group;
                    continue;
                }
                
                if (pathFoundNode is not CommandNode command)
                    throw new InvalidOperationException($"A command node must be of type {nameof(CommandNode)} or a group of type {nameof(IParentNode)}.");
                
                outerPath.Add(command.Key);

                yield return (outerPath, command);
                yield break;
            }
        }
        
        if (option.Type is not SubCommandGroup)
            throw new InvalidOperationException($"A command option must be of type {nameof(SubCommand)} or {nameof(SubCommandGroup)}.");

        outerPath.Add(option.Name);
        
        var subcommands = option.Options.Value
                                       .Select(opt => MapOptions(parent, outerPath, opt));
        
        foreach (var subcommand in subcommands.SelectMany(sc => sc))
            yield return subcommand;
    }
}