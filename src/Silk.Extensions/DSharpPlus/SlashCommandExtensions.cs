using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;

namespace Silk.Extensions.DSharpPlus
{
	public static class SlashCommandExtensions
	{
		public static Task CreateThinkingResponseAsync(this InteractionContext ctx, bool ephemeral = true)
			=> ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new() {IsEphemeral = ephemeral});
	}
}