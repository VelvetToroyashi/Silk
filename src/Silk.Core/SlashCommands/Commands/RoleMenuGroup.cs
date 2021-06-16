using System.Threading.Tasks;
using DSharpPlus.SlashCommands;

namespace Silk.Core.SlashCommands.Commands
{
	public class RoleMenuGroup : SlashCommandModule
	{
		[SlashCommandGroup("rolemenu","RoleMenu-related commands!")]
		public class RoleMenuCommands : SlashCommandModule
		{
			[SlashCommand("create", "Create a new role menu")]
			public async Task Create(InteractionContext ctx, string name, string emojis)
			{
				
			}
		}
	}
}