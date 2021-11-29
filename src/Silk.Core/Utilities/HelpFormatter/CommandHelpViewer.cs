using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OneOf;
using Remora.Commands.Conditions;
using Remora.Commands.Results;
using Remora.Commands.Services;
using Remora.Commands.Trees;
using Remora.Commands.Trees.Nodes;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Rest.Core;
using Remora.Results;

namespace Silk.Core.Utilities.HelpFormatter
{
	public class CommandHelpViewer
	{


		private readonly IDiscordRestChannelAPI _channelApi;
		private readonly CommandService _commands;
		private readonly IServiceProvider _services;
		private readonly CommandTree _tree;


		public CommandHelpViewer(CommandTree tree, CommandService commands, IServiceProvider services, IDiscordRestChannelAPI channelApi)
		{
			_tree = tree;
			_commands = commands;
			_services = services;
			_channelApi = channelApi;
		}

		public async Task<Result<IMessage>> SendHelpAsync(string? command, Snowflake channelID)
		{
			var formatter = _services.GetService<IHelpFormatter>();

			if (formatter is null)
				return Result<IMessage>.FromError(new InvalidOperationError("No help formatter was registered with the container."));

			IEmbed embed;

			if (command is null)
			{
				embed = formatter.GetHelpEmbed(_tree.Root.Children);
			}
			else
			{
				IChildNode? node = GetCommands(command, _tree.Root);

				if (node is null) //TODO: Change this message to "No command was found with the name '<name>'."
					return Result<IMessage>.FromError(new NotFoundError("The specified command does not exist."));

				embed = node is GroupNode gn ? formatter.GetHelpEmbed(gn.Children) : formatter.GetHelpEmbed((CommandNode)node);
			}

			return await _channelApi.CreateMessageAsync(channelID, embeds: new[] { embed });
		}

		public IChildNode? GetCommands(string command, IParentNode parent)
		{
			if (!command.Contains(' ')) // Top level command, only search the immediate children
				return parent.Children.FirstOrDefault(x => command.Equals(x.Key, StringComparison.OrdinalIgnoreCase) ||
				                                           x.Aliases.Contains(command, StringComparer.OrdinalIgnoreCase));
			var commandRoute = command.Split(' ');

			foreach (var token in commandRoute)
				foreach (var child in parent.Children)
					if (token.Equals(child.Key) || child.Aliases.Contains(token, StringComparer.OrdinalIgnoreCase))
						if (child is IParentNode pn)
							return GetCommands(string.Join(" ", commandRoute.Skip(1)), pn) ?? child;
						else return child;

			return null;
		}

		public async Task<OneOf<IChildNode, IEnumerable<IChildNode>>> GetApplicableCommands(IUser user, IChildNode node)
		{
			if (node is not GroupNode gn)
			{
				var res = await CheckConditionsAsync(_services, node, (node as CommandNode)!.CommandMethod, CancellationToken.None);



			}
			else { }

			return default;
		}

		private async Task<Result> CheckConditionsAsync
		(
			IServiceProvider services,
			IChildNode? node,
			ICustomAttributeProvider attributeProvider,
			CancellationToken ct
		)
		{
			var conditionAttributes = attributeProvider.GetCustomAttributes(typeof(ConditionAttribute), false);

			if (!conditionAttributes.Any())
				return Result.FromSuccess();

			foreach (var conditionAttribute in conditionAttributes)
			{
				var conditionType = typeof(ICondition<>).MakeGenericType(conditionAttribute.GetType());

				var conditionMethod = conditionType.GetMethod(nameof(ICondition<ConditionAttribute>.CheckAsync));
				if (conditionMethod is null)
					throw new InvalidOperationException();

				var conditions = services
					.GetServices(conditionType)
					.Where(c => c is not null)
					.Cast<ICondition>()
					.ToList();

				if (!conditions.Any())
					throw new InvalidOperationException("Condition attributes were applied, but no matching condition was registered.");

				foreach (var condition in conditions)
				{
					var invocationResult = conditionMethod.Invoke(condition, new[] { conditionAttribute, ct }) ?? throw new InvalidOperationException();

					var result = await (ValueTask<Result>)invocationResult;

					if (!result.IsSuccess)
						return Result.FromError(new ConditionNotSatisfiedError($"The condition \"{condition.GetType().Name}\" was not satisfied.", node), result);
				}
			}

			return Result.FromSuccess();
		}
	}
}