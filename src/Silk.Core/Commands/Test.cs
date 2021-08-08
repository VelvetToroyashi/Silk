using System;
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
					.AppendLine($"Logging channel: {(modConfig.LoggingChannel is var count and not 0 ? $"<#{count}>" : "Not set")}")
					.AppendLine()
					.AppendLine("__Invites:__")
					.AppendLine($"> Scan invite: <:_:{(modConfig.ScanInvites ? Emojis.ConfirmId : Emojis.DeclineId)}>")
					.AppendLine($"> Infract on invite: <:_:{(modConfig.WarnOnMatchedInvite ? Emojis.ConfirmId : Emojis.DeclineId)}>")
					.AppendLine($"> Delete matched invite: <:_:{(modConfig.DeleteMessageOnMatchedInvite ? Emojis.ConfirmId : Emojis.DeclineId)}>")
					.AppendLine($@"> Use agressive invite matching (`disc((ord)?(((app)?\.com\/invite)|(\.gg)))\/([A-z0-9-_]{{2,}})`): <:_:{(modConfig.UseAggressiveRegex ? Emojis.ConfirmId : Emojis.DeclineId)}>>>")
					.AppendLine($"> Allowed invites: {(modConfig.AllowedInvites.Count is 0 ? "None" : $"{modConfig.AllowedInvites.Count} allowed invites [See {ctx.Prefix}config view invites]")}")
					.AppendLine()
					.AppendLine("__Infractions:__")
					.AppendLine($"> Infraction steps: {(modConfig.InfractionSteps.Count is var dictCount and not 0 ? $"{dictCount} steps [See {ctx.Prefix}config view infractions]" : "Not configured")}")
					.AppendLine($"> Infraction steps (named): {((modConfig.NamedInfractionSteps?.Count ?? 0) is var infNameCount and not 0 ? $"{infNameCount} steps [See {ctx.Prefix}config view infractions]" : "Not configured")}")
					.AppendLine($"> Auto-escalate automod infractions: <:_:{(modConfig.AutoEscalateInfractions ? Emojis.ConfirmId : Emojis.DeclineId)}>");


				embed
					.WithTitle($"Configuration for {ctx.Guild.Name}:")
					.WithColor(DiscordColor.Azure)
					.WithDescription(contentBuilder.ToString());

				await ctx.RespondAsync(embed);
			}

			[Command]
			public async Task Greeting(CommandContext ctx)
			{
				var contentBuilder = new StringBuilder();
				var config = await _mediator.Send(new GetGuildConfigRequest(ctx.Guild.Id));

				contentBuilder
					.Clear()
					.AppendLine("__**Greeting option:**__")
					.AppendLine($"> Option: {config.GreetingOption.Humanize()}")
					.AppendLine($"> Greetting channel {(config.GreetingOption is GreetingOption.DoNotGreet ? "N/A" : $"<#{config.GreetingChannel}>")}")
					.AppendLine($"> Greeting text: {(config.GreetingOption is GreetingOption.DoNotGreet ? "N/A" : $"\n\n{config.GreetingText}")}")
					.AppendLine($"> Greeting role: {(config.GreetingOption is GreetingOption.GreetOnRole && config.VerificationRole is var role and not 0 ? $"<@&{role}>" : "N/A")}");

				var explanation = config.GreetingOption switch
				{
					GreetingOption.DoNotGreet => "I will not greet members at all.",
					GreetingOption.GreetOnJoin => "I will greet members as soon as they join",
					GreetingOption.GreetOnRole => "I will greet members when they're given a specific role",
					GreetingOption.GreetOnScreening => "I will greet members when they pass membership screening. Only applicable to community servers.",
					_ => throw new ArgumentOutOfRangeException()
				};

				contentBuilder
					.AppendLine()
					.AppendLine("**Greeting option explanation:**")
					.AppendLine(explanation);

				var embed = new DiscordEmbedBuilder()
					.WithColor(DiscordColor.Azure)
					.WithTitle($"Config for {ctx.Guild.Name}")
					.WithDescription(contentBuilder.ToString());

				await ctx.RespondAsync(embed);
			}

			[Command]
			public async Task Invites(CommandContext ctx)
			{
				var config = await _mediator.Send(new GetGuildModConfigRequest(ctx.Guild.Id));
			}
			
		}
	}
}