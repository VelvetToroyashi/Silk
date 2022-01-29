using System.Threading.Tasks;
using DSharpPlus;
using Microsoft.EntityFrameworkCore;
using RoleMenuPlugin.Database;
using YumeChan.PluginBase;

namespace RoleMenuPlugin
{
    public sealed class RoleMenuPlugin : Plugin
    {
        private readonly DiscordClient   _client;
        private readonly RoleMenuContext _db;

        private readonly RoleMenuRoleService _roleMenu;

        public RoleMenuPlugin(RoleMenuRoleService roleMenu, DiscordClient client, RoleMenuContext db)
        {
            _roleMenu = roleMenu;
            _client   = client;
            _db       = db;

            Version = GetType().Assembly.GetName().Version!.ToString(3);
        }
        public override string DisplayName => "Role-Menu Plugin";

        public override async Task LoadAsync()
        {
            await base.LoadAsync(); // Simply sets Loaded to true //

            await _db.Database.MigrateAsync();

            _client.ComponentInteractionCreated += _roleMenu.Handle;
        }

        public override async Task UnloadAsync()
        {
            await base.UnloadAsync();
            _client.ComponentInteractionCreated -= _roleMenu.Handle; // Stop listening to events. //
        }
    }
}