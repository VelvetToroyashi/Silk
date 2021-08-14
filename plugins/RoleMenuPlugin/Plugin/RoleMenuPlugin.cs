using System.Threading.Tasks;
using DSharpPlus;
using YumeChan.PluginBase;

namespace RoleMenuPlugin
{
	public sealed class RoleMenuPlugin : Plugin
	{
		public override string PluginDisplayName => "Role-Menu Plugin";
		public override bool PluginStealth => false;

		private readonly RoleMenuRoleService _roleMenu;
		private readonly DiscordShardedClient _client;
		
		public RoleMenuPlugin(RoleMenuRoleService roleMenu, DiscordShardedClient client)
		{
			_roleMenu = roleMenu;
			_client = client;
		}

		public override async Task LoadPlugin()
		{
			await base.LoadPlugin(); // Simply sets Loaded to true //
			_client.ComponentInteractionCreated += _roleMenu.Handle;
		}

		public override async Task UnloadPlugin()
		{
			await base.UnloadPlugin();
			_client.ComponentInteractionCreated -= _roleMenu.Handle; // Stop listening to events. //
		}
	}
}