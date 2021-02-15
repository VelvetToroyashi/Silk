using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
using Silk.Extensions;

namespace Silk.Core.Utilities.HelpFormatter
{
    public class HelpFormatter : BaseHelpFormatter
    {
        public Command? Command { get; private set; }
        public Command[]? Subcommands { get; private set; }

        public HelpFormatter(CommandContext ctx) : base(ctx)
        {
            Command = null;
            Subcommands = null;
        }

        public override BaseHelpFormatter WithCommand(Command command)
        {
            Command = command;
            return this;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            Subcommands = subcommands.ToArray();
            return this;
        }


        public override CommandHelpMessage Build()
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder().WithColor(DiscordColor.PhthaloBlue);

            if (Command == null)
            {
                embed.WithTitle("Silk Commands:")
                    .WithFooter("* = Group | ** = Executable group");
                IOrderedEnumerable<IGrouping<string?, Command>> modules = Subcommands!
                    .GroupBy(x => x.Module.ModuleType.GetCustomAttribute<CategoryAttribute>()?.Name)
                    .Where(x => x.Key is not null)
                    .OrderBy(x => Categories.Order.IndexOf(x.Key));

                foreach (IGrouping<string?, Command> commands in modules)
                    embed.AddField(commands.Key ?? "Uncategorized",
                        commands
                            .Select(x => $"`{x.Name}" +
                                         $"{(x is CommandGroup g ? g.IsExecutableWithoutSubcommands ? "**" : "*" : "")}`")
                            .Join(", "));
            }
            else
            {
                if (Command.IsExperimental())
                    embed.WithColor(DiscordColor.DarkRed)
                        .WithFooter("\nThis command is in testing, and marked as Experimental! Please open a ticket if it breaks.");


                IReadOnlyList<CommandArgument>? args = Command?.Overloads.OrderByDescending(x => x.Priority).FirstOrDefault()?.Arguments;

                string title = Command!.IsExperimental() ? $"[EXP] Command: `{Command!.QualifiedName}" : $"Command: `{Command!.QualifiedName}";
                var builder = new StringBuilder(title);
                if (args is not null) builder.Append(GetArgs(args));
                builder.Append('`');

                embed.WithTitle(builder.ToString()).WithDescription(Command.Description);

                if (Command.ExecutionChecks.OfType<RequireOwnerAttribute>().Any())
                    embed.AddField($"{CustomEmoji.Staff} Developer", "You can't use it!", true);
                else if (Command.IsHidden)
                    embed.AddField("\\👻 Hidden", "How did you find it?", true);

                RequireUserPermissionsAttribute? userPerms =
                    Command
                        .ExecutionChecks
                        .OfType<RequireUserPermissionsAttribute>()
                        .FirstOrDefault();

                if (userPerms is not null)
                    embed.AddField("Requires Permissions", userPerms.Permissions.ToPermissionString(), true);
                if (Command.Aliases.Any())
                    embed.AddField("Aliases", Command.Aliases.Select(x => $"`{x}`").Join(", "), true);
                if (Subcommands is not null)
                    embed.AddField("Subcommands", Subcommands.Select(x => $"`{x.QualifiedName}`").Join("\n"), true);
                if (Command.Overloads.Count > 1)
                    embed.AddField("Command overloads:", Command.Overloads
                        .Skip(1)
                        .Select(o => $"`{Command.Name} {GetArgs(o.Arguments)}`")
                        .Join("\n"));

            }

            return new(null, embed.Build());
        }

        private string GetArgs(IReadOnlyList<CommandArgument> args)
        {
            string argString = string.Empty;
            foreach (CommandArgument arg in args)
            {
                argString += arg.IsOptional ? " [" : " <";
                argString += arg.Name;
                argString += arg.IsOptional ? "]" : ">";
            }
            return argString;
        }

    }
}