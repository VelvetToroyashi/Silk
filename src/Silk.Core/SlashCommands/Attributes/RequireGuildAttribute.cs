using System.Threading.Tasks;
using DSharpPlus.SlashCommands;

namespace Silk.Core.SlashCommands.Attributes
{
	public class RequireGuildAttribute : SlashCheckBaseAttribute
	{
		public override async Task<bool> ExecuteChecksAsync(InteractionContext ctx) => ctx.Interaction.GuildId is not null;
	}
}