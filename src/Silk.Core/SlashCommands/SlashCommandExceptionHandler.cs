using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using Microsoft.Extensions.Logging;
using Silk.Core.SlashCommands.Attributes;

namespace Silk.Core.SlashCommands
{
	public sealed class SlashCommandExceptionHandler
	{
		private const string MissingCommonGuildMessage = "Sorry, but I need to share some common server with you!\n " +
		                                                 "I was only invited with slash commands, and not as a bot user, so I can't perform an action that relies on contacting you!";
		private const string MissingBotUserMessage = "Sorry, but I was only invited for slash commands! Contact a staff member and ask them to re-autheneticate me with the bot scope!";
		
		private readonly ILogger<SlashCommandExceptionHandler> _logger;
		public SlashCommandExceptionHandler(ILogger<SlashCommandExceptionHandler> logger) => _logger = logger;
		
		public async Task Handle(SlashCommandsExtension slashExt, SlashCommandErrorEventArgs args)
		{
			_logger.LogWarning(args.Exception, "An exception was thrown from the invocation of a slash command:");
			if (args.Exception is SlashExecutionChecksFailedException slchks)
			{
				if (slchks.FailedChecks[0] is RequireGuildAttribute)
					await args.Context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Sorry, but you can't execute this command in DMs!").AsEphemeral(true));
				else if (slchks.FailedChecks[0] is (RequireCommonGuildAttribute or RequireBotAttribute))
				{
					await args.Context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
						new DiscordInteractionResponseBuilder()
							.WithContent(slchks.FailedChecks[0] is RequireCommonGuildAttribute ? MissingCommonGuildMessage : MissingBotUserMessage)
							.AsEphemeral(true)
							.AddComponents(new DiscordLinkButtonComponent($"https://discord.com/oauth2/authorize?client_id={args.Context.Client.CurrentApplication.Id}&permissions=502656214&scope=bot%20applications.commands", "Invite with bot scope")));
					
				}
			}
		}
	}
}