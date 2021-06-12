using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConcurrentCollections;
using DSharpPlus;
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
		#region Class Initialization

		

		
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
			Edit = "edit",
			Main = "main";

		private readonly Dictionary<string, Func<GuildConfigService, DiscordInteraction, Task>> _compMethDict = new()
		{
			[$"{ConfigDropdown}"] = (g, i) => g.HandleDropdownAsync(i),
			[$"{Config}{Split}{Main}"] = (g, i) => Task.CompletedTask, /* TODO: Implement main menu because I'm somehow this stupid */
			[$"{Config}{Split}{Button}{Split}{Greeting}{Split}{View}"] = (g, i) => g.ViewCurrentGreetingAsync(i)
			
		};
		
		public GuildConfigService(DiscordShardedClient client, ILogger<GuildConfigService> logger, ConfigService config, IServiceCacheUpdaterService updater)
		{
			_client = client;
			_logger = logger;
			_config = config;
			_updater = updater;
			_client.ComponentInteractionCreated += HandleComponentAsync;

		}
		#endregion
		/// <summary>
		/// Presents a view for the configuration of the provided guild's Id.
		/// </summary>
		/// <param name="interaction">The slash command context to respond with.</param>
		public async Task ViewCurrentServerConfig(InteractionContext interaction) { }



		private async Task HandleComponentAsync(DiscordClient sender, ComponentInteractionCreateEventArgs e)
		{
			if (!e.Id.StartsWith(Config))
				return;
			await e.Interaction.CreateResponseAsync(InteractionResponseType.DefferedMessageUpdate);
			
			e.Handled = true;
			_activeMenus.Add(e.Message.Id);

			if (_compMethDict.TryGetValue(e.Id, out var me))
			{
				await me(this, e.Interaction);
				return;
			}

			await e.Interaction.CreateFollowupMessageAsync(new()
			{
				IsEphemeral = true,
				Content = "Sorry, but that doesn't have implemented functionality! Please contact the developers immediately."
			});
		}
		
		private Task HandleDropdownAsync(DiscordInteraction args) 
			=> args.EditOriginalResponseAsync(new() {Content = "Oh no, this hasn't been implemented yet!"});

		#region Welcome / Greeting
		
		/// <summary>
		/// Presents two options 
		/// </summary>
		/// <param name="interaction">The interaction to edit.</param>
		public async Task ShowWelcomeScreenAsync(DiscordInteraction interaction)
		{
			var builder = new DiscordWebhookBuilder();
			var components = new[]
			{
				new DiscordButtonComponent(ButtonStyle.Primary, $"{Config}{Split}{Button}{Split}{Greeting}{Split}{View}", "View current greeting config"),
				new DiscordButtonComponent(ButtonStyle.Secondary, $"{Config}{Split}{Button}{Split}{Greeting}{Split}{Edit}", "Edit current greeting config")
			};
			
			builder.WithContent("Please make a selection.");
			builder.AddComponents(components);
			
			await interaction.EditOriginalResponseAsync(builder);
		}

		private async Task ViewCurrentGreetingAsync(DiscordInteraction interaction)
		{
			var currentConfig = await _config.GetConfigAsync(interaction.GuildId.Value);
			
			// This shouldn't be possible, since the button that invokes this command is
			//disabled when the config isn't set to greet members, but we should check again anyways.
			if (!currentConfig.GreetMembers)
			{
				var builder = new DiscordWebhookBuilder();
				builder.WithContent("Sorry, but this server isn't set up to greet members...yet.");
				builder.AddComponents(
					new DiscordButtonComponent(ButtonStyle.Secondary, $"{Config}{Split}{Main}", "Return to main config menu"),
					new DiscordButtonComponent(ButtonStyle.Secondary, $"{Config}{Split}{Button}{Split}{Greeting}{Split}{View}", "View current greeting config"));
					
				await interaction.EditOriginalResponseAsync(builder);
				
			}
		}

		#endregion
		
		
	}
}