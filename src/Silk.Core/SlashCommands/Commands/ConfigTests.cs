using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using Silk.Core.Services.Server;

namespace Silk.Core.SlashCommands.Commands
{
	public class ConfigTests : SlashCommandModule
	{
		private readonly GuildConfigService _config;
		public ConfigTests(GuildConfigService config) => _config = config;

		[SlashCommand("config", "Config thing")]
		public Task ConfigTest(InteractionContext ctx)
			=> _config.DisplayMainMenuAsync(ctx.Interaction);
	}
}