using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SilkBot
{
    public static class HelpCache
    {
        public static Dictionary<string, DiscordEmbed> Entries { get; set; }

        public static void Initialize(DiscordColor color)
        {
            Entries = new Dictionary<string, DiscordEmbed>();

            var baseHelpEb = new DiscordEmbedBuilder()
            {
                Title = "Silk's Commands:",
                Color = color,
            };

            var methodSb = new StringBuilder();
            var helpSb = new StringBuilder();

            var allCommands = CommandHelper.GetAllCommands();
            var methodParameters = new List<string>();
            var sortedCommandNames = allCommands.OrderBy(e => e.GetCustomAttribute<CommandAttribute>().Name);

            foreach (var method in sortedCommandNames)
            {
                var key = "";

                //Build base Embed for the method
                var methodEmbed = new DiscordEmbedBuilder().WithColor(color);
                var methodAtt = method.GetCustomAttributes();

                foreach (var customAttribute in methodAtt)
                {
                    switch (customAttribute)
                    {
                        case CommandAttribute cmdAtt:
                            methodEmbed.WithTitle(cmdAtt.Name);
                            key = cmdAtt.Name ?? "Yeet";
                            break;

                        case HelpDescriptionAttribute dscAtt:
                            var sb = new StringBuilder();
                            sb.AppendLine(dscAtt.Description ?? "No description given");
                            methodEmbed.WithDescription(sb.ToString() ?? "No description given");
                            break;
                    }
                }
                if (key.ToLower() == "help")
                    continue;
                if (methodEmbed.Description! is null)
                    continue;
                if (!helpSb.ToString().Contains(methodEmbed.Description?.ToString()))
                    helpSb.AppendLine($"**{methodEmbed.Title}** - {methodEmbed.Description ?? ""}");
                else
                {
                    methodParameters.Add(methodEmbed.Description ?? "No description given");
                }
                var footerSb = new StringBuilder(method.GetParameters().Any(param => param.ParameterType != typeof(CommandContext)) ? "Accepted Parameters\n" : "This command takes no arguments!");
                foreach (var parameter in method.GetParameters())
                {
                    if (parameter.ParameterType == typeof(CommandContext)) continue;
                    var prAtt = parameter.GetCustomAttribute<HelpDescriptionAttribute>();

                    footerSb.Append($"- `{parameter.Name}`");

                    if (prAtt != null)
                    {
                        footerSb.Append(prAtt.Description.Any() ? $": {prAtt.Description}\n" : "No parameter description given. Sorry about that.");
                        foreach (var usage in method.GetCustomAttribute<HelpDescriptionAttribute>().ExampleUsages)
                        {
                            footerSb.AppendLine($"Example/Syntax: {usage}");
                        }
                    }
                }
                foreach (var alias in method.GetCustomAttributes<AliasesAttribute>().OrderBy(alias => alias.Aliases.OrderBy(trueAlias => trueAlias)))
                {
                    var aliasStringBuilder = new StringBuilder(alias.Aliases.Any() ? $"{alias.Aliases.First()}" : "");
                    if (alias.Aliases.Any())
                    {
                        foreach (var name in alias.Aliases)
                            aliasStringBuilder.AppendLine(name.Length > 0 ? $", {name}" : "Wut");
                    }
                    methodEmbed.WithDescription(aliasStringBuilder.ToString());
                }

                methodEmbed.WithDescription(footerSb.ToString())
                    .WithFooter("Silk", "https://cdn.discordapp.com/avatars/721514294587424888/311b3e09fa8144721c2c19b9b8ec6c31.png?size=4096")
                    .WithTimestamp(DateTime.Now);

                Entries.TryAdd(key.ToLower(), methodEmbed.Build());
            }
            baseHelpEb.WithDescription(helpSb.ToString())
                .WithFooter("Silk", "https://cdn.discordapp.com/avatars/721514294587424888/311b3e09fa8144721c2c19b9b8ec6c31.png?size=4096")
                .WithTimestamp(DateTime.Now);

            Entries.Add("help", baseHelpEb.Build());
        }
    }
}