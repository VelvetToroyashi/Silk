using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConcurrentCollections;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using Silk.Core.Services.Interfaces;

namespace Silk.Core.Services.Server
{
	/// <summary>
	/// A service class that provides API and backend functionality for per-server configuration UI handling.
	/// </summary>
	public sealed class GuildConfigService
	{
		private readonly DiscordShardedClient _client;
		private readonly ILogger<GuildConfigService> _logger;
		private readonly ConcurrentHashSet<ulong> _activeMenus = new();
		private readonly IServiceCacheUpdaterService _updater;
		private readonly ConfigService _config;


		private const string 
			Config = "cnfg",
			Button = "bttn",
			Split = "|",
			Dropdown = "drpn",
			ConfigDropdown = Config + Split + Dropdown, 
			Greeting = "grtn",
			View = "view",
			Edit = "edit";

		private readonly Dictionary<string, Func<GuildConfigService, ComponentInteractionCreateEventArgs, Task>> _compMethDict = new()
		{
			[$"{ConfigDropdown}"] = (g, i) => g.HandleDropdownAsync(i),
			[$"{ConfigDropdown}{Split}{Greeting}"] = (g, i) => g.ShowWelcomeScreenAsync(i)
		};
		
		public GuildConfigService(DiscordShardedClient client, ILogger<GuildConfigService> logger, ConfigService config, IServiceCacheUpdaterService updater)
		{
			_client = client;
			_logger = logger;
			_config = config;
			_updater = updater;
			_client.ComponentInteractionCreated += HandleComponentAsync;

		}

		/// <summary>
		/// Presents a view for the configuration of the provided guild's Id.
		/// </summary>
		/// <param name="interaction">The slash command context to respond with.</param>
		public async Task ViewCurrentServerConfig(InteractionContext interaction) { }

		/// <summary>
		/// Presents a view for the configuration of the provided guild's Id.
		/// </summary>
		/// <param name="context">The command context to respond with.</param>
		public async Task ViewCurrentServerConfig(CommandContext context) { } /* I'll implement this soon. TODO: Implment */

		private async Task HandleComponentAsync(DiscordClient sender, ComponentInteractionCreateEventArgs e)
		{
			if (!e.Id.StartsWith(Config))
				return;
			await e.Interaction.CreateResponseAsync(InteractionResponseType.DefferedMessageUpdate);
			
			e.Handled = true;
			_activeMenus.Add(e.Message.Id);

			if (_compMethDict.TryGetValue(e.Id, out var me))
			{
				await me(this, e);
				return;
			}

			await e.Interaction.CreateFollowupMessageAsync(new()
			{
				IsEphemeral = true,
				Content = "Sorry, but that doesn't have implemented functionality! Please contact the developers immediately."
			});
		}
		
		private Task HandleDropdownAsync(ComponentInteractionCreateEventArgs args) 
			=> args.Interaction.EditOriginalResponseAsync(new() {Content = "Oh no, this hasn't been implemented yet!"});

		public async Task ShowWelcomeScreenAsync(ComponentInteractionCreateEventArgs args)
		{
			var builder = new DiscordWebhookBuilder();
			var components = new[]
			{
				new DiscordButtonComponent(ButtonStyle.Primary, $"{Config}{Split}{Button}{Split}{Greeting}{Split}{View}", "View current welcome"),
				new DiscordButtonComponent(ButtonStyle.Secondary, $"{Config}{Split}{Button}{Split}{Greeting}{Split}{Edit}", "Edit current greeting")
			};
			
			builder.WithContent("Please make a selection.");
			builder.AddComponents(components);
			
			await args.Interaction.EditOriginalResponseAsync(builder);
		}
		
	}
}