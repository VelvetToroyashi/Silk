using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SilkBot
{
    public static class CommandHelper
    {
        public static IEnumerable<Type> GetAllCommandModules()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .SelectMany(a => a.GetTypes())
                .Where(t => t.BaseType == typeof(BaseCommandModule) && !t.IsAbstract);
        }

        public static IEnumerable<MethodInfo> GetAllCommands()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .SelectMany(a => a.GetTypes())
                .Where(t => t.BaseType == typeof(BaseCommandModule))
                .SelectMany(t => t.GetMethods())
                .Where(m => m.GetCustomAttribute<CommandAttribute>() != null);
        }
    }
}