using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Core.Utilities.HelpFormatter;
using Silk.Extensions;
using Silk.Shared.Constants;

namespace Silk.Core.Commands.Bot
{
	[HelpCategory(Categories.Bot)]
	public class AboutCommand : BaseCommandModule
	{
		private readonly DiscordClient _client;
		public AboutCommand(DiscordClient client)
		{
			_client = client;
		}

		[Command("about")]
		[Description("Shows relevant information, data and links about Silk!")]
		public async Task SendBotInfo(CommandContext ctx)
		{
			DiscordApplication? app = await ctx.Client.GetCurrentApplicationAsync();
			Version? dsp = typeof(DiscordClient).Assembly.GetName().Version;

			int guilds = _client.Guilds.Count;

			DiscordEmbedBuilder? embed = new DiscordEmbedBuilder()
				.WithTitle("About Silk!")
				.WithColor(DiscordColor.Gold)
				.AddField("Total guilds", $"{guilds}", true)
				.AddField("Owner(s)", app.Owners.Select(x => x.Username).Join(", "), true)
				.AddField("Bot version", StringConstants.Version, true)
				.AddField("Library", $"DSharpPlus {dsp!.Major}.{dsp.Minor}-{dsp.Revision}", true);

			var invite = $"https://discord.com/api/oauth2/authorize?client_id={ctx.Client.CurrentApplication.Id}&permissions=972418070&scope=bot%20applications.commands";
			DiscordMessageBuilder? builder = new DiscordMessageBuilder()
				.WithEmbed(embed)
				.AddComponents(
					new DiscordLinkButtonComponent(invite, "Invite Me!"),
					new DiscordLinkButtonComponent("https://github.com/VelvetThePanda/Silk", "Source Code!"),
					new DiscordLinkButtonComponent("https://discord.gg/HZfZb95", "Support Server!"))
				.AddComponents(
					new DiscordLinkButtonComponent("https://youtrack.velvetthepanda.dev/projects/dc41e8bf-975b-4108-ba22-25a04cd2f120", "Issue Tracker"),
					new DiscordLinkButtonComponent("https://youtrack.velvetthepanda.dev/issue/SBP-4", "Feature Requests"),
					new DiscordLinkButtonComponent("https://ko-fi.com/velvetthepanda", "Ko-Fi! (Donations)"));
			await ctx.RespondAsync(builder);
		}
	}
}