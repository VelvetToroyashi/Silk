using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System;

namespace SilkBot.Utilities
{
    public static class HelpCache
    {
        public static Dictionary<string, DiscordEmbed> Entries { get; } = new Dictionary<string, DiscordEmbed>();
        private static int numberOfCommands;
        private static int currentCommand;
        public static void Initialize()
        {
            Console.WriteLine("Initializing help!");
            var sw = System.Diagnostics.Stopwatch.StartNew();
            IEnumerable<MethodInfo> allCommands = CommandHelper.GetAllCommands();
            numberOfCommands = allCommands.GroupBy(g => g.Name).Count();
            DiscordEmbed helpEmbed = GenerateHelpEmbed(allCommands.OrderBy(a => a.Name.ToLower()));
            Entries.Add("help", helpEmbed);
            foreach (var commandGroup in CommandHelper.GetAllCommands().OrderBy(c => c.Name.ToLower()).GroupBy(c => c.Name.ToLower()))
            {
                var embed = GenerateHelp(commandGroup.ToArray());
                Entries.TryAdd(embed.Title.ToLower(), embed);
            }
            sw.Stop();
            Console.WriteLine($"Initialized Help Cache in {sw.ElapsedMilliseconds} ms.");
        }

        public static DiscordEmbed GenerateHelpEmbed(IEnumerable<MethodInfo> methods)
        {
            var embed = new DiscordEmbedBuilder().WithTitle("Available Commands:").WithColor(DiscordColor.CornflowerBlue);
            var sb = new StringBuilder();
            var orderedMethods = methods.OrderBy(n => n.Name.ToLower());
            //Iterate over each command; skip if cached; skip if it's not to be cached//
            for (int i = 0; i < methods.Count(); i++)
            {
                MethodInfo method = orderedMethods.ElementAt(i);
                string name = method.GetCustomAttribute<CommandAttribute>()?.Name ?? method.Name;
                var description = new Lazy<string>(() => method.GetCustomAttribute<HelpDescriptionAttribute>()?.Description ?? "Description unavailable");
                if (sb.ToString().Contains(name)) continue;
                if (HideCommand(method.GetCustomAttributes().Select(a => a.GetType()))) continue;
                sb.AppendLine($"**`{name}`** - {description.Value}");
            }
            return embed.WithDescription(sb.ToString());
        }
        private static bool HideCommand(IEnumerable<Type> type) =>
            type.Any(t => t == typeof(HiddenAttribute) || t == typeof(RequireOwnerAttribute));
        public static DiscordEmbed GenerateHelp(MethodInfo[] methods)
        {
            var embed = new DiscordEmbedBuilder().WithColor(DiscordColor.CornflowerBlue);
            var sb = new StringBuilder();
            embed.WithTitle(methods[0].GetCustomAttribute<CommandAttribute>()?.Name?.ToLower() ?? methods[0].Name.ToLower());

            methods.OrderBy(m => m.GetParameters().Length);
            foreach (var method in methods)
            {
                string commandDescription = method.GetCustomAttribute<HelpDescriptionAttribute>()?.Description ?? "No description provided";
                foreach (var param in method.GetParameters())
                {
                    if (param.ParameterType == typeof(DSharpPlus.CommandsNext.CommandContext))
                    {
                        continue;
                    }
                    sb.AppendLine($"`{param.Name.ToLower()}` - {param.GetCustomAttribute<HelpDescriptionAttribute>()?.Description ?? "Description unavaible"}");
                }
                sb.AppendLine();
            }
            if (sb.ToString().Length < 10) sb.Append("This command takes no arguments.");
            embed.WithDescription(sb.ToString());
            Console.WriteLine($"\u001b[34m[{++currentCommand}/{numberOfCommands}] \u001b[37mCreated help embed for command!");
            return embed.Build();
        }

    }
}

