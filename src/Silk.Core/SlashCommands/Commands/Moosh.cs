using System.Threading.Tasks;
using DSharpPlus.SlashCommands;

namespace Silk.Core.SlashCommands.Commands
{
	public class Moosh : SlashCommandModule
	{
		[SlashCommand("dropshop", "Moosh's dropshop!")]
		public async Task MooshShop(InteractionContext ctx){}
	}
}