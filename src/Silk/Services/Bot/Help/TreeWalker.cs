using System;
using System.Collections.Generic;
using System.Linq;
using Remora.Commands.Services;
using Remora.Commands.Trees.Nodes;

namespace Silk.Services.Bot.Help;

public class TreeWalker
{
    private readonly CommandService _commands;
    
    public TreeWalker(CommandService commands)
    {
        _commands = commands;
    }
    
    public IReadOnlyList<IChildNode> FindNodes(string? name, string? treeName = null)
    { 
        if (!_commands.TreeAccessor.TryGetNamedTree(treeName, out var tree))
            return Array.Empty<IChildNode>();

        if (string.IsNullOrEmpty(name))
            return tree.Root.Children;
        
        var stack = new Stack<string>(name.Split(' ').Reverse());
        
        return FindNodesCore(stack, tree.Root).ToArray();
    }

    private IEnumerable<IChildNode> FindNodesCore(Stack<string> stack, IParentNode parent)
    {
        // For loop would probably be more efficient, but I like the idea of popping
        // tokens off a stack. Small perf diff; barely even noticeable.

        GetNextToken(out var current);

        IParentNode? next = null;
        
        foreach (var child in parent.Children)
        {
            if (
                child.Key.Equals(current, StringComparison.OrdinalIgnoreCase) ||
                child.Aliases.Contains(current, StringComparer.OrdinalIgnoreCase)
               )
            {
                if (TokensRemain())
                {
                    if (child is not IParentNode nextLevel)
                        continue;

                    next = nextLevel;
                    break;
                }
                    
                yield return child;
                continue;
            }
        }
        
        if (!TokensRemain() || next is null)
            yield break;
        
        foreach (var child in FindNodesCore(stack, next))
            yield return child;

        bool TokensRemain() => stack.TryPeek(out _);
        bool GetNextToken(out string next) => stack.TryPop(out next);
    }
}