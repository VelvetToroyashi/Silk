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
using Silk.Shared.Constants;

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
					.AppendLine("__Greeting:__ ")
					.AppendLine($"> Option: {config.GreetingOption.Humanize()}")
					.AppendLine($"> Greetting channel {(config.GreetingOption is GreetingOption.DoNotGreet ? "N/A" : $"<#{config.GreetingChannel}>")}")
					.AppendLine($"> Greeting text: {(config.GreetingOption is GreetingOption.DoNotGreet ? "N/A" : $"[See {ctx.Prefix}config view greeting]")}")
					.AppendLine()
					.AppendLine()
					.AppendLine("**Moderation Config:**")
					.AppendLine($"Max role mentions: {GetCountString(modConfig.MaxRoleMentions)}")
					.AppendLine($"Max user mentions: {GetCountString(modConfig.MaxUserMentions)}")
					.AppendLine()
					.AppendLine($"Mute role: {(modConfig.MuteRoleId is 0 ? "Not set" : $"<@&{modConfig.MuteRoleId}>")}")
					.AppendLine($"Logging channel: {(modConfig.LoggingChannel is 0 ? "Not set" : $"<#{modConfig.LoggingChannel}>")}")
					.AppendLine()
					.AppendLine("__Invites:__")
					.AppendLine($"> Scan invite: <:_:{(modConfig.ScanInvites ? Emojis.ConfirmId : Emojis.DeclineId)}>")
					.AppendLine($"> Infract on invite: <:_:{(modConfig.WarnOnMatchedInvite ? Emojis.ConfirmId : Emojis.DeclineId)}>")
					.AppendLine($"> Delete matched invite: <:_:{(modConfig.DeleteMessageOnMatchedInvite ? Emojis.ConfirmId : Emojis.DeclineId)}>")
					.AppendLine($"> Allowed invites: {(modConfig.AllowedInvites.Count is 0 ? "None" : $"{modConfig.AllowedInvites.Count} allowed invites [See {ctx.Prefix}config view invites]")}");
				
				embed
					.WithTitle($"Configuration for {ctx.Guild.Name}:")
					.WithColor(DiscordColor.Azure)
					.WithDescription(contentBuilder.ToString());

				await ctx.RespondAsync(embed);
			}
		}
	}
}