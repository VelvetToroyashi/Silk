using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace TestPlugin
{
	public class Command : BaseCommandModule
	{
		[Command]
		public async Task Test(CommandContext ctx)
		{
			await ctx.RespondAsync("Pog! This command is registerd in a plugin, and does not have to be shipped with the base image!");
		}
	}
}