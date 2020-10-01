using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SilkBot.Utilities
{
    public static class HelpCache
    {
        public static Dictionary<string, DiscordEmbed> Entries { get; } = new Dictionary<string, DiscordEmbed>();
        public static void Initialize()
        {
            
            DiscordEmbed helpEmbed = GenerateHelpEmbed(CommandHelper.GetAllCommands().OrderBy(a => a.Name.ToLower()));
            Entries.Add("help", helpEmbed);
        }

        public static DiscordEmbed GenerateHelpEmbed(IEnumerable<MethodInfo> methods)
        {
            var embed = new DiscordEmbedBuilder().WithColor(DiscordColor.CornflowerBlue);
            var sb = new StringBuilder();
            methods.OrderBy(n => n.Name.ToLower());
            for (int i = 0; i < methods.Count(); i++)
            {
                MethodInfo method = methods.ElementAt(i);
                if (sb.ToString().Contains(method.GetCustomAttribute<CommandAttribute>()?.Name ?? method.Name)) continue;
                if (method.GetCustomAttributes().Any(att => att.GetType() == typeof(HiddenAttribute) || att.GetType() == typeof(RequireOwnerAttribute))) continue;
                sb.AppendLine($"**`{method.GetCustomAttribute<CommandAttribute>().Name}`** - {method.GetCustomAttribute<HelpDescriptionAttribute>()?.Description ?? "No description provided"}");
            }
            embed.WithTitle("Available commands:").WithDescription(sb.ToString());
            return embed;
        }

    }
}

