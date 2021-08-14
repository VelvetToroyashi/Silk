using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace TestPlugin
{
	public class TestCommandModule : BaseCommandModule
	{
		[Command]
		public Task TestOne(CommandContext ctx)
			=> ctx.RespondAsync("This command comes from a plugin! It no references to the base project.");

		[Command]
		public Task TestTwo(CommandContext ctx)
			=> ctx.RespondAsync("This also comes from a plugin, woo!");
	}
}