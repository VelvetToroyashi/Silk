using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using Microsoft.Extensions.Logging;
using Silk.Core.SlashCommands.Attributes;

namespace Silk.Core.SlashCommands
{
	public class SlashCommandExceptionHandler
	{
		private readonly ILogger<SlashCommandExceptionHandler> _logger;
		public SlashCommandExceptionHandler(ILogger<SlashCommandExceptionHandler> logger) => _logger = logger;
		
		public async Task Handle(SlashCommandsExtension slashExt, SlashCommandErrorEventArgs args)
		{
			_logger.LogWarning(args.Exception, "An exception was thrown from the invocation of a slash command:");
			if (args.Exception is SlashExecutionChecksFailedException slchks)
			{
				if (slchks.FailedChecks[0] is RequireGuildAttribute)
					await args.Context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Sorry, but you can't execute this command in DMs!").AsEphemeral(true));
			}
		}
	}
}