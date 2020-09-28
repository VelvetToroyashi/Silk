using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.EntityFrameworkCore.Update.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;

namespace SilkBot.Utilities
{
    public static class HelpCache
    {
        public static Dictionary<string, DiscordEmbed> Entries { get; set; }
        private static readonly List<(string, string)> _commands = new List<(string, string)>();


        public static void Initialize()
        {
            var commands = CommandHelper.GetAllCommands().OrderBy(c => c.GetCustomAttribute<CommandAttribute>().Name);
            foreach(var command in commands)
            {
                var method = AddCommandHelp(command, DiscordColor.CornflowerBlue);
                
            }
            

        }

        private static (string MethodName, string Parameters, DiscordEmbed CommandEmbed) AddCommandHelp(MethodInfo command, DiscordColor color)
        {
            var embed = new DiscordEmbedBuilder().WithColor(color);
            var name = command.GetCustomAttribute<CommandAttribute>().Name;
            var description = command.GetCustomAttribute<DescriptionAttribute>().Description;
            var parameters = command.GetParameters();

            return ("", "", null);
        }
    }
}

