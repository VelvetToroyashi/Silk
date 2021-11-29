using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Remora.Commands.Trees.Nodes;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Silk.Extensions;

namespace Silk.Core.Utilities.HelpFormatter
{
	public class HelpFormatter : IHelpFormatter
	{
		/// <inheritdoc/>
		public IEmbed GetHelpEmbed(CommandNode command)
		{
			string? commandUsage = GetUsage(command);

			string usage = commandUsage is null ? "" : $"Usage: `{commandUsage}`\n";
			string aliases = command.Aliases.Any() ? string.Join(", ", command.Aliases) + '\n' : "None";

			var parameterHelp = command
				.Shape
				.Parameters
				.Select(p => (IEmbedField)new EmbedField((p.IsOmissible() ? "(Optional) " : "") + p.HintName, p.Description));

			var embed = new Embed
			{
				Title = $"Help for {command.Key}",
				Description = usage + command.Shape.Description,
				Colour = Color.DodgerBlue,
				Fields = new[]
					{
						new EmbedField("Aliases", aliases) //We use an array vs .Prepend() because there may be more to come.
					}.Concat(parameterHelp)
					.ToList()
			};

			return embed;
		}

		/// <inheritdoc/>
		public IEmbed GetHelpEmbed(IEnumerable<IChildNode> subcommands)
		{
			IEmbed embed;
			var commandString = string.Join('\n', subcommands.Select(c => '`' + c.Key + '`'));

			var fields = new List<IEmbedField>();

			var categories = subcommands
				.GroupBy(x => x is CommandNode cn
					? cn.GroupType.GetCustomAttribute<HelpCategoryAttribute>()?.Name
					: ((x as IParentNode).Children.FirstOrDefault() as CommandNode)?
					.GroupType.GetCustomAttribute<HelpCategoryAttribute>()
					?.Name)
				.OrderBy(x => Categories.Order.IndexOf(x.Key ?? Categories.Uncategorized));

			fields
				.AddRange(categories
					.Select(c => new EmbedField(c.Key ?? Categories.Uncategorized, c
						.Select(cn => $"`{cn.Key}`")
						.Join(", "))));

			if (subcommands.First().Parent is RootNode)
			{
				// Root node, display all top level commands
				embed = new Embed
				{
					Title = "Help",
					Description = "Wanna see more information? Try specifing a command!",
					Colour = Color.DodgerBlue,
					Fields = fields
				};
			}
			else
			{
				var parent = subcommands.First().Parent as IChildNode;

				embed = new Embed
				{
					Title = $"Help for {parent!.Key}:",
					Description = "Showing subcommands. \n" +
					              "Specify a command name to see more information. \n" +
					              commandString,
					Colour = Color.DodgerBlue,
					Fields = new[]
					{
						new EmbedField("Aliases", string.Join(", ", parent.Aliases), true),
						new EmbedField("Subcommands", commandString, true)
					}
				};
			}

			return embed;
		}

		private string? GetUsage(CommandNode command)
			=> !command.Shape.Parameters.Any()
				? null
				: string.Join(' ', command.Shape.Parameters.Select(p => p.IsOmissible() ? $"[{p.HintName}]" : $"<{p.HintName}>"));
	}
}