using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MediatR;

namespace Silk.Core.Commands.Server.Config
{
	public class ConfigCommand : BaseCommandModule
	{
		private readonly IMediator _mediator;
		private readonly DiscordShardedClient _client;
		private readonly ConcurrentDictionary<ulong, ulong> _menus = new();
		
		public ConfigCommand(IMediator mediator, DiscordShardedClient client)
		{
			_mediator = mediator;
			_client = client;
			_client.ComponentInteractionCreated += HandleConfigButton;
		}
		~ConfigCommand() => _client.ComponentInteractionCreated -= HandleConfigButton;
		
		
		private async Task HandleConfigButton(DiscordClient c, ComponentInteractionCreateEventArgs e)
		{
			if (!e.Id.StartsWith("config"))
				return;

			if (e.Id.Split(' ').Last() != e.User.Id.ToString())
			{
				await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate, new() {Content = "Aha, this isn't your menu, silly!", IsEphemeral = true});
			}

			var task = e.Id.Split(' ')[1] switch
			{
				"main" => new MainConfigMenu().EditAsync(e.Message),
				_ => Task.CompletedTask
			};
			
		}

		private record MainConfigMenu
		{
			private readonly DiscordSelectComponentOption[] _options = new[]
			{
				new DiscordSelectComponentOption("Greeting settings", "greeting", "Edit greeting-related settings"),
				new DiscordSelectComponentOption("Moderation settings", "moderation", "Edit moderation-related settings")
			};
			
			private readonly DiscordMessageBuilder _builder;

			public MainConfigMenu()
			{
				DiscordSelectComponent select = new("main-select", "Guild Settings", _options);
				_builder = new DiscordMessageBuilder()
					.WithContent("Please make a selection.")
					.AddComponents(select);
			}
			
			public Task<DiscordMessage> EditAsync(DiscordMessage msg) => msg.ModifyAsync(_builder);
			public Task<DiscordMessage> SendAsync(DiscordChannel chn) => chn.SendMessageAsync(_builder);
		}
	}
}