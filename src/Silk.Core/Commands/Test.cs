using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Humanizer;
using MediatR;
using Silk.Core.Data.MediatR.Guilds;
using Silk.Core.Data.MediatR.Guilds.Config;
using Silk.Core.Data.Models;
using Silk.Core.Utilities;

namespace Silk.Core.Commands
{
	[RequireFlag(UserFlag.Staff)]
	[Group("config")]
	public class TestConfigModule : BaseCommandModule
	{
		private readonly IMediator _mediator;
		public TestConfigModule(IMediator mediator) => _mediator = mediator;

		[Group("view")]
		public sealed class ViewConfigModule : BaseCommandModule
		{
			private readonly IMediator _mediator;
			public ViewConfigModule(IMediator mediator) => _mediator = mediator;

			private string GetCountString(int count) => count is 0 ? "Not set/enabled" : count.ToString();
			
			
			[GroupCommand]
			public async Task View(CommandContext ctx)
			{
				var config = await _mediator.Send(new GetGuildConfigRequest(ctx.Guild.Id));
				var modConfig = await _mediator.Send(new GetGuildModConfigRequest(ctx.Guild.Id));
				
				var embed = new DiscordEmbedBuilder();
				var contentBuilder = new StringBuilder();

				contentBuilder
					.Clear()
					.AppendLine("**General Config:**")
					.AppendLine($"Greeting option: {config.GreetingOption.Humanize()}")
					.AppendLine($"> Greetting channel {(config.GreetingOption is GreetingOption.DoNotGreet ? "N/A" : $"<#{config.GreetingChannel}>")}")
					.AppendLine($"> Greeting text: {(config.GreetingOption is GreetingOption.DoNotGreet ? "N/A" : $"[See {ctx.Prefix}config view greeting]")}")
					.AppendLine()
					.AppendLine()
					.AppendLine("**Moderation Config:**")
					.AppendLine($"Max role mentions: {GetCountString(modConfig.MaxRoleMentions)}")
					.AppendLine($"Max user mentions: {GetCountString(modConfig.MaxUserMentions)}")
					.AppendLine()
					.AppendLine($"Mute role: {(modConfig.MuteRoleId is 0 ? "Not set" : $"<@&{modConfig.MuteRoleId}>")}")
					.AppendLine($"Logging channel: {(modConfig.LoggingChannel is 0 ? "Not set" : $"<#{modConfig.LoggingChannel}>")}");
				
				embed
					.WithTitle($"Configuration for {ctx.Guild.Name}:")
					.WithColor(DiscordColor.Azure)
					.WithDescription(contentBuilder.ToString());

				await ctx.RespondAsync(embed);
			}
		}
	}
}