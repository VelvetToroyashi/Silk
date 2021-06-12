using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using Silk.Core.Services.Server;
using Silk.Extensions.DSharpPlus;

namespace Silk.Core.SlashCommands.Commands
{
	public class ConfigTests : SlashCommandModule
	{
		private readonly GuildConfigService _config;
		public ConfigTests(GuildConfigService config) => _config = config;

		[SlashCommand("config", "Config thing")]
		public async Task ConfigTest(InteractionContext ctx)
		{
			await ctx.CreateThinkingResponseAsync(false);
			await _config.ShowWelcomeScreenAsync(ctx.Interaction);
		}
	}
}