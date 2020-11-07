using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SilkBot.Utilities
{
    public static class CommandHelper
    {
        public static IEnumerable<Type> GetAllCommandModules(Assembly asm)
        {
            return asm.GetTypes()
                .Where(t => t.BaseType == typeof(BaseCommandModule) && !t.IsAbstract);
        }

        public static IEnumerable<MethodInfo> GetAllCommands(Assembly asm)
        {
            return asm.GetTypes()
                .Where(t => t.BaseType == typeof(BaseCommandModule))
                .SelectMany(t => t.GetMethods())
                .Where(m => m.GetCustomAttribute<CommandAttribute>() != null);
        }

    }
}
