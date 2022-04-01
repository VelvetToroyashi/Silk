using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Remora.Commands.Extensions;
using Remora.Commands.Groups;
using Remora.Discord.Gateway.Extensions;
using Remora.Discord.Gateway.Responders;

namespace Silk.Extensions.Remora;

public static class RemoraIServiceCollectionExtensions
{
    public static IServiceCollection AddResponders(this IServiceCollection collection, Assembly assembly)
    {
        IEnumerable<Type>? types = assembly
                                  .GetExportedTypes()
                                  .Where(t => t.IsClass && !t.IsAbstract && t.IsResponder());

        foreach (Type type in types)
        {
            var responderGroup = type.GetCustomAttribute<ResponderGroupAttribute>()?.Group ?? ResponderGroup.Normal;
           
            collection.AddResponder(type, responderGroup);
        }
            

        return collection;
    }

    public static IServiceCollection AddCommands(this IServiceCollection collection, Assembly assembly)
    {
        IEnumerable<Type>? types = assembly
                                  .GetExportedTypes()
                                  .Where(t => t.IsClass && !t.IsNested && !t.IsAbstract && t.IsAssignableTo(typeof(CommandGroup)));

        var tree = collection.AddCommandTree();
        
        foreach (Type type in types.Where(t => t.GetCustomAttribute<SlashCommandAttribute>() is null))
            tree.WithCommandGroup(type);
        
        return tree.Finish();
    }
    
    public static IServiceCollection AddSlashCommands(this IServiceCollection collection, Assembly assembly)
    {
        IEnumerable<Type>? types = assembly
                                  .GetExportedTypes()
                                  .Where(t => t.IsClass && !t.IsNested && !t.IsAbstract && t.IsAssignableTo(typeof(CommandGroup)));

        var tree = collection.AddCommandTree("silk_slash_tree");
        
        foreach (Type type in types.Where(t => t.GetCustomAttribute<SlashCommandAttribute>() is not null))
            tree.WithCommandGroup(type);
        
        return tree.Finish();
    }
}